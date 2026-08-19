using System.Text.Json.Serialization;

namespace ImageProcessor.Web.Models;

public sealed record CreateBatchRequestDto
{
    public required IReadOnlyList<CreateBatchImageRequestDto> Images { get; init; }
}

public sealed record CreateBatchImageRequestDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required IReadOnlyList<ImageOperationDto> Operations { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResizeOperationDto), "Resize")]
[JsonDerivedType(typeof(CropOperationDto), "Crop")]
[JsonDerivedType(typeof(RotateOperationDto), "Rotate")]
public abstract record ImageOperationDto;

public sealed record ResizeOperationDto : ImageOperationDto
{
    public required int Width { get; init; }
    public required int Height { get; init; }
}

public sealed record CropOperationDto : ImageOperationDto
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}

public sealed record RotateOperationDto : ImageOperationDto
{
    public required double Degrees { get; init; }
}
