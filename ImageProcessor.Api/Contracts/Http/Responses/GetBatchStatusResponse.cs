using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Api.Contracts.Http.Responses;

public sealed record GetBatchStatusResponse
{
    public required Guid Id { get; init; }
    public required BatchStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}
