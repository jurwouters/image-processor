using System.Text;
using System.Text.Json;
using ImageProcessor.Application.Messaging;
using ImageProcessor.Domain.Operations;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ImageProcessor.Infrastructure.Messaging;

public class RabbitMqProcessingQueue(
    IOptions<RabbitMqSettings> settings, 
    RabbitMqChannelFactory channelFactory) 
    : IProcessingQueue, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new ImageOperationJsonConverter() }
    };

    public async ValueTask EnqueueAsync(ImageProcessingTask task, CancellationToken cancellationToken = default)
    {
        var channel = await channelFactory.GetChannelAsync(cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(task, _jsonOptions));

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _settings.QueueName,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: cancellationToken);
    }

    public ValueTask DisposeAsync() => channelFactory.DisposeAsync();
}
