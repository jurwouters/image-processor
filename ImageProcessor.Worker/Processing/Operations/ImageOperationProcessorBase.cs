using ImageProcessor.Domain.Operations;
using SixLabors.ImageSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public abstract class ImageOperationProcessorBase<TOperation> : IImageOperationProcessor
    where TOperation : ImageOperation
{
    public Type OperationType => typeof(TOperation);

    public Task ProcessAsync(Image image, ImageOperation operation, CancellationToken cancellationToken = default)
    {
        if (operation is not TOperation typedOperation)
        {
            throw new ArgumentException(
                $"Expected operation type '{typeof(TOperation).Name}' but got '{operation.GetType().Name}'.",
                nameof(operation));
        }

        return ProcessTypedAsync(image, typedOperation, cancellationToken);
    }

    protected abstract Task ProcessTypedAsync(Image image, TOperation operation, CancellationToken cancellationToken);
}
