using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Application.Repositories;

public interface IImageRepository : IRepository<Image>
{
    Task<Image?> GetByIdWithBatchAsync(
        Guid batchId,
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task<bool> HasIncompleteImagesInBatchAsync(
        Guid batchId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}
