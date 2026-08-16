using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Api.Contracts.Http.Requests;

public record CreateBatchRequest
{
    public required IReadOnlyList<ImageMetadata> ImagesMetadata { get; init; }
    public required IReadOnlyList<ImageOperation> Operations { get; init; }
}

public record ImageMetadata
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}