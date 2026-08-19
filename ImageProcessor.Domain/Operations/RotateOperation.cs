namespace ImageProcessor.Domain.Operations;

[ImageOperationType("Rotate")]
public sealed record RotateOperation : ImageOperation
{
    public required double Degrees { get; init; }
}
