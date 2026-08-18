namespace ImageProcessor.Web.Models;

public sealed record GetBatchStatusResponseDto
{
    public required Guid Id { get; init; }
    public required int Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<GetBatchImageStatusDto> Images { get; init; }
}

public sealed record GetBatchImageStatusDto
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string S3Key { get; init; }
    public required int Status { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? DownloadUrl { get; init; }
    public DateTime? DownloadUrlExpiresAtUtc { get; init; }
}
