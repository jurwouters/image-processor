using ImageProcessor.Domain.Entities;

namespace ImageProcessor.Api.Contracts.Http.Responses;

public record StartBatchResponse
{
    public required Guid Id { get; init; }
    public required BatchStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}
