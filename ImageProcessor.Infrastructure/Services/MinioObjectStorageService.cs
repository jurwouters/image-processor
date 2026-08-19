using ImageProcessor.Application.Services;
using ImageProcessor.Application.Services.Models.Storage;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace ImageProcessor.Infrastructure.Services;

public sealed class MinioObjectStorageService(IMinioClient client, IConfiguration configuration) : IObjectStorageService
{
    private readonly string _bucket = configuration["S3:BucketName"] 
        ?? throw new InvalidOperationException("S3:BucketName missing");

    public async Task<PresignedUploadResult> CreatePresignedUploadAsync(
        Guid batchId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var sanitizedFileName = Path.GetFileName(fileName);
        var s3Key = $"batches/{batchId}/{Guid.NewGuid():N}-{sanitizedFileName}";
        const int expirySeconds = 600;

        var uploadUrl = await client.PresignedPutObjectAsync(
            new PresignedPutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(s3Key)
                .WithExpiry(expirySeconds));

        return new PresignedUploadResult
        {
            S3Key = s3Key,
            UploadUrl = uploadUrl,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expirySeconds),
            FileName = sanitizedFileName,
            ContentType = contentType
        };
    }

    public async Task<UploadedObjectMetadata?> GetObjectMetadataAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        try
        {
            var objectInfo = await client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(s3Key),
                cancellationToken);

            return new UploadedObjectMetadata
            {
                ContentType = objectInfo.ContentType ?? "application/octet-stream",
                FileSize = objectInfo.Size,
                LastModifiedUtc = objectInfo.LastModified.ToUniversalTime()
            };
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task<Stream> GetObjectStreamAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(s3Key);

        var memoryStream = new MemoryStream();

        await client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(s3Key)
                .WithCallbackStream((stream, ct) => stream.CopyToAsync(memoryStream, ct)),
            cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task UploadObjectAsync(
        string s3Key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(s3Key);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(s3Key)
                .WithObjectSize(content.Length)
                .WithStreamData(content)
                .WithContentType(contentType),
            cancellationToken);
    }

    public Task DeleteObjectAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(s3Key);

        return client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_bucket)
                .WithObject(s3Key),
            cancellationToken);
    }
}
