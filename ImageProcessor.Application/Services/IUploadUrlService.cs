using ImageProcessor.Application.Services.Models.Storage;

namespace ImageProcessor.Application.Services;

public interface IUploadUrlService
{
    Task<PresignedUploadResult> CreatePresignedUploadAsync(
        Guid batchId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<PresignedDownloadResult> CreatePresignedDownloadAsync(
        string s3Key,
        CancellationToken cancellationToken = default);

    Task<UploadedObjectMetadata?> GetUploadedObjectMetadataAsync(
        string s3Key,
        CancellationToken cancellationToken = default);
}
