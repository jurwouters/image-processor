using ImageProcessor.Application.Messaging;

namespace ImageProcessor.Worker.Processing;

public sealed class ImageProcessingHandler(ILogger<ImageProcessingHandler> logger) : ITaskHandler
{
    public async Task HandleAsync(ImageProcessingTask task, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Received task — BatchId: {BatchId}, ImageId: {ImageId}, File: {FileName}, Operations: {Operations}",
            task.BatchId,
            task.ImageId,
            task.FileName,
            string.Join(", ", task.Operations.Select(o => o.GetType().Name)));

        await Task.Delay(5000);

        return;
    }
}
