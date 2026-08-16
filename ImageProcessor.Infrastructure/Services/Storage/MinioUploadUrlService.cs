using ImageProcessor.Application.Services.Models.Storage;
using ImageProcessor.Application.Services.Storage;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace ImageProcessor.Infrastructure.Services.Storage;

public sealed class MinioUploadUrlService(IMinioClient client, IConfiguration configuration) : IUploadUrlService
{
    public async Task<PresignedUploadResult> CreatePresignedUploadAsync(
        Guid batchId,
        string fileName, 
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var bucket = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName missing");
        var sanitizedFileName = Path.GetFileName(fileName);
        var s3Key = $"batches/{batchId}/{Guid.NewGuid():N}-{sanitizedFileName}";
        var expirySeconds = 600;

        var uploadUrl = await client.PresignedPutObjectAsync(
            new PresignedPutObjectArgs()
                .WithBucket(bucket)
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

    public async Task<UploadedObjectMetadata?> GetUploadedObjectMetadataAsync(
        string s3Key,
        CancellationToken cancellationToken = default)
    {
        var bucket = configuration["S3:BucketName"] ?? throw new InvalidOperationException("S3:BucketName missing");

        try
        {
            var objectInfo = await client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(bucket)
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
}
