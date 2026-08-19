using ImageProcessor.Application.Services.Models.BatchService;
using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Application.Services;

public interface IBatchService
{
    Task<BatchResult> CreateBatchAsync(
        Guid batchId,
        IReadOnlyList<ImageOperation> operations,
        IReadOnlyList<RegisterExpectedImageCommand> expectedImages,
        CancellationToken cancellationToken = default);

    Task<BatchResult?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchResult?> StartBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchImageResult?> GetBatchImageAsync(Guid batchId, Guid imageId, CancellationToken cancellationToken = default);
}
