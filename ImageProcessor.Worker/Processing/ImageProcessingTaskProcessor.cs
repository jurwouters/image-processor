using ImageProcessor.Application.Messaging;
using ImageProcessor.Application.Services;
using ImageProcessor.Domain.Operations;
using ImageProcessor.Worker.Processing.Operations;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing;

public sealed class ImageProcessingTaskProcessor(
    IImageOperationProcessorResolver processorResolver,
    IObjectStorageService objectStorage,
    IImageProcessingStateService processingStateService,
    ILogger<ImageProcessingTaskProcessor> logger) : IImageProcessingTaskProcessor
{
    private const string ProcessedContentType = "image/png";
    private const int PngQuality = 100;

    public async Task HandleAsync(ImageProcessingTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task.Operations);

        logger.LogInformation(
            "Received task — BatchId: {BatchId}, ImageId: {ImageId}, File: {FileName}, Operations: {Operations}",
            task.BatchId,
            task.ImageId,
            task.FileName,
            string.Join(", ", task.Operations.Select(o => o.GetType().Name)));

        var imageRecord = await processingStateService.GetImageRecordAsync(task.BatchId, task.ImageId, cancellationToken);
        if (imageRecord is null)
        {
            return;
        }

        await processingStateService.MarkAsProcessingAsync(imageRecord, cancellationToken);

        try
        {
            var processedS3Key = BuildProcessedKey(imageRecord.S3Key);
            await ProcessImageContentAsync(imageRecord.S3Key, processedS3Key, task.Operations, cancellationToken);
            await processingStateService.MarkAsCompletedAsync(
                task.BatchId,
                task.ImageId,
                imageRecord,
                processedS3Key,
                cancellationToken);

            logger.LogInformation(
                "Finished processing image {ImageId} in batch {BatchId}.",
                task.ImageId,
                task.BatchId);
        }
        catch (Exception ex)
        {
            await processingStateService.MarkAsFailedAsync(imageRecord, cancellationToken);
            logger.LogError(ex, "Failed processing image {ImageId} in batch {BatchId}.", task.ImageId, task.BatchId);
            throw;
        }
    }

    private async Task ProcessImageContentAsync(
        string sourceS3Key,
        string processedS3Key,
        IReadOnlyList<ImageOperation> operations,
        CancellationToken cancellationToken)
    {
        await using var originalStream = await objectStorage.GetObjectStreamAsync(sourceS3Key, cancellationToken);
        var image = SKBitmap.Decode(originalStream)
            ?? throw new InvalidOperationException("Unable to decode source image.");

        try
        {
            image = await ApplyOperationsAsync(image, operations, cancellationToken);
            await UploadProcessedImageAsync(processedS3Key, image, cancellationToken);
            await objectStorage.DeleteObjectAsync(sourceS3Key, cancellationToken);
        }
        finally
        {
            image.Dispose();
        }
    }

    private async Task<SKBitmap> ApplyOperationsAsync(
        SKBitmap source,
        IReadOnlyList<ImageOperation> operations,
        CancellationToken cancellationToken)
    {
        var image = source;

        foreach (var operation in operations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var processor = processorResolver.Resolve(operation);
            var transformed = await processor.ProcessAsync(image, operation, cancellationToken);
            ArgumentNullException.ThrowIfNull(transformed);

            if (ReferenceEquals(transformed, image))
            {
                continue;
            }

            image.Dispose();
            image = transformed;
        }

        return image;
    }

    private async Task UploadProcessedImageAsync(string s3Key, SKBitmap image, CancellationToken cancellationToken)
    {
        using var skImage = SKImage.FromBitmap(image);
        using var encoded = skImage.Encode(SKEncodedImageFormat.Png, PngQuality)
            ?? throw new InvalidOperationException("Unable to encode processed image as PNG.");

        await using var encodedStream = encoded.AsStream();
        await objectStorage.UploadObjectAsync(s3Key, encodedStream, ProcessedContentType, cancellationToken);
    }

    private static string BuildProcessedKey(string originalS3Key)
    {
        var directory = Path.GetDirectoryName(originalS3Key)?.Replace('\\', '/');
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalS3Key);
        var processedFileName = $"{fileNameWithoutExtension}-processed.png";

        return string.IsNullOrWhiteSpace(directory)
            ? processedFileName
            : $"{directory}/{processedFileName}";
    }
}
