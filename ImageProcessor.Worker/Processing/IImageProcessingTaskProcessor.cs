using ImageProcessor.Application.Messaging;

namespace ImageProcessor.Worker.Processing;

public interface IImageProcessingTaskProcessor
{
    Task HandleAsync(ImageProcessingTask task, CancellationToken cancellationToken = default);
}
