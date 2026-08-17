using ImageProcessor.Application.Messaging;
using ImageProcessor.Worker.Processing.Operations;
using ImageProcessor.Worker.Storage;
using SixLabors.ImageSharp;

namespace ImageProcessor.Worker.Processing;

public sealed class ImageProcessingHandler(
    ILogger<ImageProcessingHandler> logger,
    IImageOperationProcessorResolver processorResolver,
    IImageObjectStorage imageStorage) : ITaskHandler
{
    public async Task HandleAsync(ImageProcessingTask task, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Received task — BatchId: {BatchId}, ImageId: {ImageId}, File: {FileName}, Operations: {Operations}",
            task.BatchId,
            task.ImageId,
            task.FileName,
            string.Join(", ", task.Operations.Select(o => o.GetType().Name)));

        await using var originalStream = await imageStorage.DownloadAsync(task.S3Key, cancellationToken);
        using var image = await Image.LoadAsync(originalStream, cancellationToken);

        foreach (var operation in task.Operations)
        {
            var processor = processorResolver.Resolve(operation);
            await processor.ProcessAsync(image, operation, cancellationToken);
        }

        await using var processedStream = new MemoryStream();
        await image.SaveAsPngAsync(processedStream, cancellationToken);
        await imageStorage.UploadAsync(task.S3Key, processedStream, "image/png", cancellationToken);

        logger.LogInformation(
            "Finished processing image {ImageId} in batch {BatchId}.",
            task.ImageId,
            task.BatchId);
    }
}
