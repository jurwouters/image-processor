using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Application.Repositories;

public interface IImageRepository : IRepository<Image>
{
    Task<Image?> GetByIdWithBatchAndImagesAsync(
        Guid batchId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}
