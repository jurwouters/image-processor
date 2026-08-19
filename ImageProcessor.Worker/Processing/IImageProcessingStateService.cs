using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Worker.Processing;

public interface IImageProcessingStateService
{
    Task<Image?> GetImageRecordAsync(Guid batchId, Guid imageId, CancellationToken cancellationToken = default);

    Task MarkAsProcessingAsync(Image imageRecord, CancellationToken cancellationToken = default);

    Task MarkAsCompletedAsync(
        Guid batchId,
        Guid imageId,
        Image imageRecord,
        string processedS3Key,
        CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(Image imageRecord, CancellationToken cancellationToken = default);
}
