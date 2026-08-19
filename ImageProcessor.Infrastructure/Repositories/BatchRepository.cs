using ImageProcessor.Application.Repositories;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Infrastructure.Repositories;

public sealed class BatchRepository(ApplicationDbContext db)
    : Repository<Batch>(db), IBatchRepository
{
    public Task<Batch?> GetByIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return Set
            .AsNoTracking()
            .SingleOrDefaultAsync(batch => batch.Id == batchId, cancellationToken);
    }

    public Task<Batch?> GetByIdWithImagesAsync(
        Guid batchId,
        bool asNoTracking,
        CancellationToken cancellationToken = default)
    {
        var query = Set
            .Include(batch => batch.Images)
            .Where(batch => batch.Id == batchId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }
}
