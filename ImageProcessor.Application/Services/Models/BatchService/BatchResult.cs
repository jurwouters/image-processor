using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Application.Services.Models.BatchService;

public sealed record BatchResult
{
    public required Guid Id { get; init; }
    public required BatchStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed record BatchImageResult
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string S3Key { get; init; }
    public required ImageStatus Status { get; init; }
    public DateTime? ProcessedAt { get; init; }
}
