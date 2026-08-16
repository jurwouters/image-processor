using ImageProcessor.Worker.Messaging;
using ImageProcessor.Worker.Processing;

namespace ImageProcessor.Worker;

public sealed class Worker(
    IProcessingQueueConsumer consumer,
    ITaskHandler taskHandler,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in consumer.ReadAsync(stoppingToken))
        {
            try
            {
                await taskHandler.HandleAsync(message.Payload, stoppingToken);
                await message.AcknowledgeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process task. Requeueing.");
                await message.RejectAsync(requeue: true, stoppingToken);
            }
        }
    }
}