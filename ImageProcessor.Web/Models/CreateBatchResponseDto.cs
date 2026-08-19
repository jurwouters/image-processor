namespace ImageProcessor.Web.Models;

public sealed record CreateBatchResponseDto
{
    public required Guid Id { get; init; }
    public required int Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<PresignedUploadDto> PresignedUploads { get; init; }
}

public sealed record PresignedUploadDto
{
    public required Guid Id { get; init; }
    public required string S3Key { get; init; }
    public required string UploadUrl { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}
