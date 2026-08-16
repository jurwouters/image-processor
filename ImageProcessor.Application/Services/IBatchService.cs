using ImageProcessor.Application.Services.Models.BatchService;

namespace ImageProcessor.Application.Services;

public interface IBatchService
{
    Task<BatchResult> CreateBatchAsync(CreateBatchCommand command, CancellationToken cancellationToken = default);
    Task<BatchResult?> GetBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchResult?> StartBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
}
