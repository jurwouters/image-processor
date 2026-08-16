using ImageProcessor.Application.Messaging;

namespace ImageProcessor.Worker.Processing;

public interface ITaskHandler
{
    Task HandleAsync(ImageProcessingTask payload, CancellationToken cancellationToken = default);
}
