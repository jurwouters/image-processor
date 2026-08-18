using ImageProcessor.Application.Repositories;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Infrastructure.Repositories;

public sealed class BatchRepository(ApplicationDbContext db)
    : EfRepository<Batch>(db), IBatchRepository
{
    public async Task<Batch?> GetByIdWithImagesAsync(
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

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
