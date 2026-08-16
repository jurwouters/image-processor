using ImageProcessor.Application.Messaging;
using ImageProcessor.Application.Services;
using ImageProcessor.Application.Services.Models.BatchService;
using ImageProcessor.Application.Services.Storage;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Infrastructure.Services.Batch;

public sealed class BatchService(
    ApplicationDbContext db,
    IProcessingQueue processingQueue,
    IUploadUrlService uploadService) : IBatchService
{
    public async Task<BatchResult> CreateBatchAsync(CreateBatchCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ExpectedImages.Count == 0)
        {
            throw new ArgumentException("At least one expected image is required.");
        }

        var batch = new Domain.Entities.Batch
        {
            Id = command.Id,
            Operations = [..command.Operations],
            Status = BatchStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        var expectedImages = command.ExpectedImages.Select(image => new Image
        {
            Id = Guid.NewGuid(),
            BatchId = command.Id,
            S3Key = image.S3Key,
            FileName = image.FileName,
            ContentType = image.ContentType,
            FileSize = 0,
            Status = ImageStatus.PendingUpload,
            UploadedAt = null
        });

        db.Batches.Add(batch);
        db.Images.AddRange(expectedImages);
        await db.SaveChangesAsync(cancellationToken);

        return ToResult(batch);
    }

    public async Task<BatchResult?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await db.Batches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

        return batch is null
            ? null
            : ToResult(batch);
    }

    public async Task<BatchResult?> StartBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await db.Batches
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

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
        await db.SaveChangesAsync(cancellationToken);

        return ToResult(batch);
    }

    private static BatchResult ToResult(Domain.Entities.Batch batch) => new()
    {
        Id = batch.Id,
        Status = batch.Status,
        CreatedAt = batch.CreatedAt,
    };
}
