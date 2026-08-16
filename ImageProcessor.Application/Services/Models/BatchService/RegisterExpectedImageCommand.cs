namespace ImageProcessor.Application.Services.Models.BatchService;

public sealed record RegisterExpectedImageCommand
{
    public required string S3Key { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}
