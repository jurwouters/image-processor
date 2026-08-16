namespace ImageProcessor.Web.Models;

public sealed record StartBatchResponseDto
{
    public required Guid Id { get; init; }
    public required int Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}
