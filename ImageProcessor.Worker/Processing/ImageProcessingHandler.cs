using ImageProcessor.Application.Messaging;
using ImageProcessor.Application.Repositories;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Worker.Processing.Operations;
using ImageProcessor.Worker.Storage;
using SkiaSharp;

namespace ImageProcessor.Worker.Processing;

public sealed class ImageProcessingHandler(
    ILogger<ImageProcessingHandler> logger,
    IImageOperationProcessorResolver processorResolver,
    IImageObjectStorage imageStorage,
    IImageRepository imageRepository) : ITaskHandler
{
    public async Task HandleAsync(ImageProcessingTask task, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Received task — BatchId: {BatchId}, ImageId: {ImageId}, File: {FileName}, Operations: {Operations}",
            task.BatchId,
            task.ImageId,
            task.FileName,
            string.Join(", ", task.Operations.Select(o => o.GetType().Name)));

        var imageRecord = await imageRepository.GetByIdWithBatchAndImagesAsync(
            task.BatchId,
            task.ImageId,
            cancellationToken);

        if (imageRecord is null)
        {
            logger.LogWarning("Image record not found for BatchId {BatchId} and ImageId {ImageId}.", task.BatchId, task.ImageId);
            return;
        }

        imageRecord.Status = ImageStatus.Processing;
        if (imageRecord.Batch.Status is BatchStatus.Created or BatchStatus.Queued)
        {
            imageRecord.Batch.Status = BatchStatus.Processing;
            imageRecord.Batch.StartedAt ??= DateTime.UtcNow;
        }

        await imageRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var processedS3Key = BuildProcessedKey(imageRecord.S3Key);

            await using var originalStream = await imageStorage.DownloadAsync(imageRecord.S3Key, cancellationToken);
            var image = SKBitmap.Decode(originalStream)
                ?? throw new InvalidOperationException("Unable to decode source image.");

            try
            {
                foreach (var operation in task.Operations)
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

                await using var processedStream = new MemoryStream();
                using var skImage = SKImage.FromBitmap(image);
                using var encoded = skImage.Encode(SKEncodedImageFormat.Png, 100)
                    ?? throw new InvalidOperationException("Unable to encode processed image as PNG.");

                await processedStream.WriteAsync(encoded.ToArray(), cancellationToken);
                await imageStorage.UploadAsync(processedS3Key, processedStream, "image/png", cancellationToken);
                await imageStorage.DeleteAsync(imageRecord.S3Key, cancellationToken);
            }
            finally
            {
                image.Dispose();
            }

            imageRecord.S3Key = processedS3Key;
            imageRecord.Status = ImageStatus.Completed;
            imageRecord.ProcessedAt = DateTime.UtcNow;

            if (imageRecord.Batch.Images.All(i => i.Status == ImageStatus.Completed))
            {
                imageRecord.Batch.Status = BatchStatus.Completed;
                imageRecord.Batch.CompletedAt = DateTime.UtcNow;
            }

            await imageRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Finished processing image {ImageId} in batch {BatchId}.",
                task.ImageId,
                task.BatchId);
        }
        catch (Exception ex)
        {
            imageRecord.Status = ImageStatus.Failed;
            imageRecord.Batch.Status = BatchStatus.Failed;
            imageRecord.Batch.CompletedAt = DateTime.UtcNow;
            await imageRepository.SaveChangesAsync(cancellationToken);

            logger.LogError(ex, "Failed processing image {ImageId} in batch {BatchId}.", task.ImageId, task.BatchId);
            throw;
        }
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
