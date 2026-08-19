using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Api.Contracts.Http.Requests;

public record CreateBatchRequest
{
    public required IReadOnlyList<CreateBatchImageRequest> Images { get; init; }
}

public record CreateBatchImageRequest
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required IReadOnlyList<ImageOperation> Operations { get; init; }
}
