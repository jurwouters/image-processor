using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Application.Services.Models.BatchService;

public sealed record BatchResult
{
    public required Guid Id { get; init; }
    public required BatchStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}
