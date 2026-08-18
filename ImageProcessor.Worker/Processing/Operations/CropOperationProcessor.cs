using ImageProcessor.Domain.Operations;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public sealed class CropOperationProcessor(ILogger<CropOperationProcessor> logger)
    : ImageOperationProcessorBase<CropOperation>
{
    protected override Task<SKBitmap> ProcessTypedAsync(SKBitmap image, CropOperation operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (operation.Width <= 0 || operation.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operation),
                "Crop width and height must be greater than zero.");
        }

        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new InvalidOperationException("Cannot crop an empty image.");
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

        var croppedImage = new SKBitmap(width, height, image.ColorType, image.AlphaType, image.ColorSpace);
        using var canvas = new SKCanvas(croppedImage);

        var sourceRect = new SKRectI(x, y, x + width, y + height);
        var destinationRect = new SKRect(0, 0, width, height);
        canvas.DrawBitmap(image, sourceRect, destinationRect);

        return Task.FromResult(croppedImage);
    }
}
