using ImageProcessor.Application.Services.Models.Storage;

namespace ImageProcessor.Application.Services;

public interface IObjectStorageService
{
    Task<PresignedUploadResult> CreatePresignedUploadAsync(
        Guid batchId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<UploadedObjectMetadata?> GetObjectMetadataAsync(
        string s3Key,
        CancellationToken cancellationToken = default);

    Task<Stream> GetObjectStreamAsync(
        string s3Key,
        CancellationToken cancellationToken = default);

    Task UploadObjectAsync(
        string s3Key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteObjectAsync(
        string s3Key,
        CancellationToken cancellationToken = default);
}
