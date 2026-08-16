namespace ImageProcessor.Api.Contracts.Http.Responses;

public record PresignedUploadResponse
{
    public required string S3Key { get; init; }
    public required string UploadUrl { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}
