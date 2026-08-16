namespace ImageProcessor.Domain.Operations;

[ImageOperationType("Resize")]
public sealed record ResizeOperation : ImageOperation
{
    public required int Width { get; init; }
    public required int Height { get; init; }
}
