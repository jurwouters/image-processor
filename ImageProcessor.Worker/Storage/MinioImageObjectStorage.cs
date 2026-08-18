using Minio;
using Minio.DataModel.Args;

namespace ImageProcessor.Worker.Storage;

public sealed class MinioImageObjectStorage(IMinioClient client, IConfiguration configuration) : IImageObjectStorage
{
    public async Task<Stream> DownloadAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        var bucket = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName missing");

        var output = new MemoryStream();

        await client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(s3Key)
                .WithCallbackStream(stream => stream.CopyTo(output)),
            cancellationToken);

        output.Position = 0;
        return output;
    }

    public async Task UploadAsync(string s3Key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var bucket = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName missing");

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(s3Key)
                .WithObjectSize(content.Length)
                .WithStreamData(content)
                .WithContentType(contentType),
            cancellationToken);
    }

    public async Task DeleteAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        var bucket = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName missing");

        await client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(s3Key),
            cancellationToken);
    }
}
