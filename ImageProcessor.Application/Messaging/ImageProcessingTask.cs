using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Application.Messaging;

public readonly record struct ImageProcessingTask(
    Guid BatchId,
    Guid ImageId,
    string S3Key,
    string FileName,
    IReadOnlyList<ImageOperation> Operations);