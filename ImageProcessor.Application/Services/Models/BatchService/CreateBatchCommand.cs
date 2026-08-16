using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Application.Services.Models.BatchService;

public sealed record CreateBatchCommand
{
    public required Guid Id { get; init; }
    public required IReadOnlyList<ImageOperation> Operations { get; init; }
    public required IReadOnlyList<RegisterExpectedImageCommand> ExpectedImages { get; init; }
}
