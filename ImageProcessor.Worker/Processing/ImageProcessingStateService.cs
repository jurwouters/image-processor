using ImageProcessor.Application.Repositories;
using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Worker.Processing;

public sealed class ImageProcessingStateService(
    IImageRepository imageRepository,
    ILogger<ImageProcessingStateService> logger) : IImageProcessingStateService
{
    public async Task<Image?> GetImageRecordAsync(Guid batchId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var imageRecord = await imageRepository.GetByIdWithBatchAsync(batchId, imageId, cancellationToken);

        if (imageRecord is null)
        {
            logger.LogWarning("Image record not found for BatchId {BatchId} and ImageId {ImageId}.", batchId, imageId);
            return null;
        }

        return imageRecord;
    }

    public async Task MarkAsProcessingAsync(Image imageRecord, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageRecord);

        imageRecord.Status = ImageStatus.Processing;

        if (imageRecord.Batch.Status is BatchStatus.Created or BatchStatus.Queued)
        {
            imageRecord.Batch.Status = BatchStatus.Processing;
            imageRecord.Batch.StartedAt ??= DateTime.UtcNow;
        }

        await imageRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsCompletedAsync(
        Guid batchId,
        Guid imageId,
        Image imageRecord,
        string processedS3Key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageRecord);
        ArgumentException.ThrowIfNullOrWhiteSpace(processedS3Key);

        imageRecord.S3Key = processedS3Key;
        imageRecord.Status = ImageStatus.Completed;
        imageRecord.ProcessedAt = DateTime.UtcNow;

        var hasIncompleteImages = await imageRepository.HasIncompleteImagesInBatchAsync(batchId, imageId, cancellationToken);

        if (!hasIncompleteImages)
        {
            imageRecord.Batch.Status = BatchStatus.Completed;
            imageRecord.Batch.CompletedAt = DateTime.UtcNow;
        }

        await imageRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(Image imageRecord, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageRecord);

        imageRecord.Status = ImageStatus.Failed;
        imageRecord.Batch.Status = BatchStatus.Failed;
        imageRecord.Batch.CompletedAt = DateTime.UtcNow;

        await imageRepository.SaveChangesAsync(cancellationToken);
    }
}
