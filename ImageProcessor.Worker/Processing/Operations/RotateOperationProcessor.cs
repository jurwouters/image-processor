using ImageProcessor.Domain.Operations;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing.Operations;

public sealed class RotateOperationProcessor(ILogger<RotateOperationProcessor> logger)
    : ImageOperationProcessorBase<RotateOperation>
{
    private const double Epsilon = 0.0000001d;

    protected override Task<SKBitmap> ProcessTypedAsync(SKBitmap image, RotateOperation operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new InvalidOperationException("Cannot rotate an empty image.");
        }

        logger.LogInformation(
            "Applying rotate operation. Degrees: {Degrees}",
            operation.Degrees);

        var angleRadians = DegreesToRadians(operation.Degrees);
        using var rotatedImage = RotateIntoExpandedCanvas(image, operation.Degrees);

        var (safeWidth, safeHeight) = CalculateLargestSafeRectangle(image.Width, image.Height, angleRadians);
        var cropWidth = Math.Clamp((int)Math.Floor(safeWidth), 1, rotatedImage.Width);
        var cropHeight = Math.Clamp((int)Math.Floor(safeHeight), 1, rotatedImage.Height);

        var cropOriginX = (rotatedImage.Width - cropWidth) / 2;
        var cropOriginY = (rotatedImage.Height - cropHeight) / 2;

        var croppedImage = new SKBitmap(cropWidth, cropHeight, image.ColorType, image.AlphaType, image.ColorSpace);
        using var cropCanvas = new SKCanvas(croppedImage);
        cropCanvas.DrawBitmap(rotatedImage, -cropOriginX, -cropOriginY);

        return Task.FromResult(croppedImage);
    }

    private static SKBitmap RotateIntoExpandedCanvas(SKBitmap image, double angleDegrees)
    {
        var angleRadians = DegreesToRadians(angleDegrees);
        var sine = Math.Abs(Math.Sin(angleRadians));
        var cosine = Math.Abs(Math.Cos(angleRadians));

        var expandedWidth = Math.Max(1, (int)Math.Ceiling((image.Width * cosine) + (image.Height * sine)));
        var expandedHeight = Math.Max(1, (int)Math.Ceiling((image.Width * sine) + (image.Height * cosine)));

        var rotatedImage = new SKBitmap(expandedWidth, expandedHeight, image.ColorType, image.AlphaType, image.ColorSpace);
        using var canvas = new SKCanvas(rotatedImage);
        using var paint = new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
        };

        var samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        using var sourceImage = SKImage.FromBitmap(image);

        canvas.Clear(SKColors.Transparent);
        canvas.Translate(expandedWidth / 2f, expandedHeight / 2f);
        canvas.RotateDegrees((float)angleDegrees);
        canvas.Translate(-image.Width / 2f, -image.Height / 2f);
        canvas.DrawImage(sourceImage, 0, 0, samplingOptions, paint);

        return rotatedImage;
    }

    private static (double Width, double Height) CalculateLargestSafeRectangle(int sourceWidth, int sourceHeight, double angleRadians)
    {
        var width = (double)sourceWidth;
        var height = (double)sourceHeight;
        var angle = Math.Abs(angleRadians % Math.PI);

        if (angle > Math.PI / 2d)
        {
            angle = Math.PI - angle;
        }

        var sine = Math.Abs(Math.Sin(angle));
        var cosine = Math.Abs(Math.Cos(angle));

        if (sine < Epsilon)
        {
            return (width, height);
        }

        if (cosine < Epsilon)
        {
            return (height, width);
        }

        var widthIsLonger = width >= height;
        var longerSide = widthIsLonger ? width : height;
        var shorterSide = widthIsLonger ? height : width;

        double safeWidth;
        double safeHeight;

        if (shorterSide <= (2d * sine * cosine * longerSide) || Math.Abs(sine - cosine) < Epsilon)
        {
            var halfShorterSide = shorterSide * 0.5d;

            if (widthIsLonger)
            {
                safeWidth = halfShorterSide / sine;
                safeHeight = halfShorterSide / cosine;
            }
            else
            {
                safeWidth = halfShorterSide / cosine;
                safeHeight = halfShorterSide / sine;
            }
        }
        else
        {
            var cosineDoubleAngle = (cosine * cosine) - (sine * sine);
            safeWidth = ((width * cosine) - (height * sine)) / cosineDoubleAngle;
            safeHeight = ((height * cosine) - (width * sine)) / cosineDoubleAngle;
        }

        return (Math.Abs(safeWidth), Math.Abs(safeHeight));
    }

    private static double DegreesToRadians(double degrees)
        => degrees * (Math.PI / 180d);
}
