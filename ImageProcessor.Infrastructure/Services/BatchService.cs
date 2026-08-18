using ImageProcessor.Application.Messaging;
using ImageProcessor.Application.Repositories;
using ImageProcessor.Application.Services;
using ImageProcessor.Application.Services.Models.BatchService;
using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Infrastructure.Services;

public sealed class BatchService(
    IBatchRepository batchRepository,
    IProcessingQueue processingQueue,
    IUploadUrlService uploadService) : IBatchService
{
    public async Task<BatchResult> CreateBatchAsync(CreateBatchCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ExpectedImages.Count == 0)
        {
            throw new ArgumentException("At least one expected image is required.");
        }

        var batch = new Batch
        {
            Id = command.Id,
            Operations = [..command.Operations],
            Status = BatchStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        var expectedImages = command.ExpectedImages
            .Select(image => new Image
            {
                Id = Guid.NewGuid(),
                BatchId = command.Id,
                S3Key = image.S3Key,
                FileName = image.FileName,
                ContentType = image.ContentType,
                FileSize = 0,
                Status = ImageStatus.PendingUpload,
                UploadedAt = null
            })
            .ToList();

        batch.Images = expectedImages;

        await batchRepository.AddAsync(batch, cancellationToken);

        return ToResult(batch);
    }

    public async Task<BatchResult?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await batchRepository.GetByIdWithImagesAsync(batchId, asNoTracking: true, cancellationToken);

        if (batch is null)
        {
            return null;
        }

        return ToResult(batch);
    }

    public async Task<BatchResult?> StartBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await batchRepository.GetByIdWithImagesAsync(batchId, asNoTracking: false, cancellationToken);

        if (batch is null)
        {
            return null;
        }

        if (batch.Status != BatchStatus.Created)
        {
            throw new InvalidOperationException($"Batch cannot be started from status '{batch.Status}'.");
        }

        if (batch.Images.Count == 0)
        {
            throw new InvalidOperationException("Batch has no expected images registered.");
        }

        foreach (var image in batch.Images)
        {
            var uploadedObject = await uploadService.GetUploadedObjectMetadataAsync(image.S3Key, cancellationToken);

            if (uploadedObject is null)
            {
                throw new InvalidOperationException($"Image '{image.FileName}' was not uploaded yet.");
            }

            if (!string.Equals(image.ContentType, uploadedObject.ContentType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Uploaded image metadata does not match for '{image.FileName}'.");
            }

            if (uploadedObject.FileSize <= 0)
            {
                throw new InvalidOperationException($"Uploaded image '{image.FileName}' is empty.");
            }

            image.FileSize = uploadedObject.FileSize;
            image.UploadedAt = uploadedObject.LastModifiedUtc;
            image.Status = ImageStatus.Uploaded;
        }

        foreach (var image in batch.Images)
        {
            var task = new ImageProcessingTask(
                batch.Id,
                image.Id,
                image.S3Key,
                image.FileName,
                batch.Operations);

            await processingQueue.EnqueueAsync(task, cancellationToken);
        }

        batch.Status = BatchStatus.Queued;

        await batchRepository.UpdateAsync(batch, cancellationToken);

        return ToResult(batch);
    }

    private static BatchResult ToResult(Batch batch) => new()
    {
        Id = batch.Id,
        Status = batch.Status,
        CreatedAt = batch.CreatedAt,
        Images = batch.Images
            .OrderBy(image => image.FileName)
            .Select(image => new BatchImageResult
            {
                Id = image.Id,
                FileName = image.FileName,
                S3Key = image.S3Key,
                Status = image.Status,
                ProcessedAt = image.ProcessedAt
            })
            .ToArray()
    };
}
