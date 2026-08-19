using ImageProcessor.Application.Services.Models.BatchService;

namespace ImageProcessor.Application.Services;

public interface IBatchService
{
    Task<BatchResult> CreateBatchAsync(
        Guid batchId,
        IReadOnlyList<RegisterExpectedImageCommand> expectedImages,
        CancellationToken cancellationToken = default);

    Task<BatchResult?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchResult?> StartBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchImageResult?> GetBatchImageAsync(Guid batchId, Guid imageId, CancellationToken cancellationToken = default);
}
