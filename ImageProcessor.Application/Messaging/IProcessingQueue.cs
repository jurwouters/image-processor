namespace ImageProcessor.Application.Messaging;

public interface IProcessingQueue
{
    ValueTask EnqueueAsync(ImageProcessingTask task, CancellationToken cancellationToken = default);
}