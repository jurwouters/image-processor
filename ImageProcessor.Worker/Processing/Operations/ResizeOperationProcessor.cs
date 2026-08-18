using ImageProcessor.Domain.Operations;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public sealed class ResizeOperationProcessor(ILogger<ResizeOperationProcessor> logger)
    : ImageOperationProcessorBase<ResizeOperation>
{
    protected override Task<SKBitmap> ProcessTypedAsync(SKBitmap image, ResizeOperation operation, CancellationToken cancellationToken)
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

        var resizedImage = image.Resize(
            new SKImageInfo(operation.Width, operation.Height, image.ColorType, image.AlphaType, image.ColorSpace),
            SKSamplingOptions.Default);

        if (resizedImage is null)
        {
            throw new InvalidOperationException("Unable to resize image.");
        }

        return Task.FromResult(resizedImage);
    }
}
