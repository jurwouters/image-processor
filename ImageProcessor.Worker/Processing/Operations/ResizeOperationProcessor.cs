using ImageProcessor.Domain.Operations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Worker.Processing.Operations;

public sealed class ResizeOperationProcessor(ILogger<ResizeOperationProcessor> logger)
    : ImageOperationProcessorBase<ResizeOperation>
{
    protected override Task ProcessTypedAsync(Image image, ResizeOperation operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (operation.Width <= 0 || operation.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operation),
                "Resize width and height must be greater than zero.");
        }

        logger.LogInformation(
            "Applying resize operation. Width: {Width}, Height: {Height}",
            operation.Width,
            operation.Height);

        image.Mutate(context => context.Resize(operation.Width, operation.Height));

        return Task.CompletedTask;
    }
}
