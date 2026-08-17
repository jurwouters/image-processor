using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Worker.Processing.Operations;

public interface IImageOperationProcessorResolver
{
    IImageOperationProcessor Resolve(ImageOperation operation);
}
