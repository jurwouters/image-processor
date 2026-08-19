using ImageProcessor.Application.Repositories;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Infrastructure.Repositories;

public sealed class ImageRepository(ApplicationDbContext db)
    : Repository<Image>(db), IImageRepository
{
    public Task<Image?> GetByIdWithBatchAsync(
        Guid batchId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        return Set
            .Include(image => image.Batch)
            .FirstOrDefaultAsync(
                image => image.Id == imageId && image.BatchId == batchId,
                cancellationToken);
    }

    public Task<bool> HasIncompleteImagesInBatchAsync(
        Guid batchId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        return Set.AnyAsync(
            image => image.BatchId == batchId
                && image.Id != imageId
                && image.Status != ImageStatus.Completed,
            cancellationToken);
    }
}
