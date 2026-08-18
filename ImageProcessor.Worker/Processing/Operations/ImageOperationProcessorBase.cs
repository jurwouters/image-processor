using ImageProcessor.Domain.Operations;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public abstract class ImageOperationProcessorBase<TOperation> : IImageOperationProcessor
    where TOperation : ImageOperation
{
    public Type OperationType => typeof(TOperation);

    public Task<SKBitmap> ProcessAsync(SKBitmap image, ImageOperation operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(operation);

        if (operation is not TOperation typedOperation)
        {
            throw new ArgumentException(
                $"Expected operation type '{typeof(TOperation).Name}' but got '{operation.GetType().Name}'.",
                nameof(operation));
        }

        return ProcessTypedAsync(image, typedOperation, cancellationToken);
    }

    protected abstract Task<SKBitmap> ProcessTypedAsync(SKBitmap image, TOperation operation, CancellationToken cancellationToken);
}
