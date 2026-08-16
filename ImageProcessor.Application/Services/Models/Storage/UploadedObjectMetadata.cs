namespace ImageProcessor.Application.Services.Models.Storage;

public sealed record UploadedObjectMetadata
{
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required DateTime LastModifiedUtc { get; init; }
}
