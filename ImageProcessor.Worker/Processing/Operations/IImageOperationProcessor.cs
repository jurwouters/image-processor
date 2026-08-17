using ImageProcessor.Domain.Operations;
using SixLabors.ImageSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public interface IImageOperationProcessor
{
    Type OperationType { get; }

    Task ProcessAsync(Image image, ImageOperation operation, CancellationToken cancellationToken = default);
}
