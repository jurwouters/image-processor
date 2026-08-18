using ImageProcessor.Application.Repositories;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Infrastructure.Repositories;

public sealed class ImageRepository(ApplicationDbContext db)
    : EfRepository<Image>(db), IImageRepository
{
    public Task<Image?> GetByIdWithBatchAndImagesAsync(
        Guid batchId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        return Set
            .Include(image => image.Batch)
            .ThenInclude(batch => batch.Images)
            .FirstOrDefaultAsync(
                image => image.Id == imageId && image.BatchId == batchId,
                cancellationToken);
    }
}
