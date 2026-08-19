using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Application.Repositories;

public interface IBatchRepository : IRepository<Batch>
{
    Task<Batch?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<Batch?> GetByIdWithImagesAsync(
        Guid batchId,
        bool asNoTracking,
        CancellationToken cancellationToken = default);
}
