namespace ImageProcessor.Worker.Storage;

public interface IImageObjectStorage
{
    Task<Stream> DownloadAsync(string s3Key, CancellationToken cancellationToken = default);

    Task UploadAsync(string s3Key, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string s3Key, CancellationToken cancellationToken = default);
}
