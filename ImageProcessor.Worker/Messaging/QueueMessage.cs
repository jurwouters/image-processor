using ImageProcessor.Application.Messaging;

namespace ImageProcessor.Worker.Messaging;

public sealed class QueueMessage(
    ImageProcessingTask payload,
    Func<CancellationToken, Task> acknowledge,
    Func<bool, CancellationToken, Task> reject)
{
    public ImageProcessingTask Payload { get; } = payload;

    public Task AcknowledgeAsync(CancellationToken cancellationToken = default)
        => acknowledge(cancellationToken);

    public Task RejectAsync(bool requeue = true, CancellationToken cancellationToken = default)
        => reject(requeue, cancellationToken);
}