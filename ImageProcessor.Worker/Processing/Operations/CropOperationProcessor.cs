using ImageProcessor.Domain.Operations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessor.Worker.Processing.Operations;

public sealed class CropOperationProcessor(ILogger<CropOperationProcessor> logger)
    : ImageOperationProcessorBase<CropOperation>
{
    protected override Task ProcessTypedAsync(Image image, CropOperation operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (operation.Width <= 0 || operation.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operation),
                "Crop width and height must be greater than zero.");
        }

        var x = Math.Clamp(operation.X, 0, image.Width - 1);
        var y = Math.Clamp(operation.Y, 0, image.Height - 1);
        var width = Math.Clamp(operation.Width, 1, image.Width - x);
        var height = Math.Clamp(operation.Height, 1, image.Height - y);

        logger.LogInformation(
            "Applying crop operation. X: {X}, Y: {Y}, Width: {Width}, Height: {Height}",
            x,
            y,
            width,
            height);

        image.Mutate(context => context.Crop(new Rectangle(x, y, width, height)));

        return Task.CompletedTask;
    }
}
