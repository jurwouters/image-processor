namespace ImageProcessor.Worker.Messaging;

public interface IProcessingQueueConsumer
{
    IAsyncEnumerable<QueueMessage> ReadAsync(CancellationToken cancellationToken);
}