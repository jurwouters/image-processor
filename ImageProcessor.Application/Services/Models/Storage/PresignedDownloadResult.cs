namespace ImageProcessor.Application.Services.Models.Storage;

public sealed record PresignedDownloadResult
{
    public required string S3Key { get; init; }
    public required string DownloadUrl { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
