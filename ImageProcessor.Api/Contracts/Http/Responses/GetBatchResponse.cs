using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Api.Contracts.Http.Responses;

public sealed record GetBatchResponse
{
    public required Guid Id { get; init; }
    public required BatchStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<GetBatchImageResponse> Images { get; init; }
}

public sealed record GetBatchImageResponse
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string S3Key { get; init; }
    public required ImageStatus Status { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? DownloadUrl { get; init; }
    public DateTime? DownloadUrlExpiresAtUtc { get; init; }
}
