using ImageProcessor.Domain.Operations;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public interface IImageOperationProcessor
{
    Type OperationType { get; }

    Task<SKBitmap> ProcessAsync(SKBitmap image, ImageOperation operation, CancellationToken cancellationToken = default);
}
