using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Application.Repositories;

public interface IBatchRepository : IRepository<Batch>
{
    Task<Batch?> GetByIdWithImagesAsync(
        Guid batchId,
        bool asNoTracking,
        CancellationToken cancellationToken = default);
}
