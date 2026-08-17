using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Worker.Processing.Operations;

public sealed class ImageOperationProcessorResolver(IEnumerable<IImageOperationProcessor> processors)
    : IImageOperationProcessorResolver
{
    private readonly Dictionary<Type, IImageOperationProcessor> processorMap =
        processors.ToDictionary(processor => processor.OperationType);

    public IImageOperationProcessor Resolve(ImageOperation operation)
    {
        if (!processorMap.TryGetValue(operation.GetType(), out var processor))
        {
            throw new NotSupportedException($"No processor registered for operation type '{operation.GetType().Name}'.");
        }

        return processor;
    }
}
