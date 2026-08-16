using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ImageProcessor.Application.Messaging;
using ImageProcessor.Domain.Operations;
using ImageProcessor.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;

namespace ImageProcessor.Worker.Messaging;

public sealed class RabbitMqProcessingQueueConsumer(
    IOptions<RabbitMqSettings> settings,
    RabbitMqChannelFactory channelFactory) : IProcessingQueueConsumer
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new ImageOperationJsonConverter() }
    };

    public async IAsyncEnumerable<QueueMessage> ReadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = await channelFactory.GetChannelAsync(cancellationToken);
        var pipe = Channel.CreateBounded<QueueMessage>(1);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var task = JsonSerializer.Deserialize<ImageProcessingTask>(
                Encoding.UTF8.GetString(ea.Body.Span), _jsonOptions);

            var message = new QueueMessage(
                task,
                ct => channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct).AsTask(),
                (requeue, ct) => channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: requeue, cancellationToken: ct).AsTask());

            await pipe.Writer.WriteAsync(message, cancellationToken);
        };

        await channel.BasicConsumeAsync(
            queue: _settings.QueueName,
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: cancellationToken);

        await foreach (var message in pipe.Reader.ReadAllAsync(cancellationToken))
            yield return message;
    }
}