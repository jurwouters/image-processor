using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ImageProcessor.Infrastructure.Messaging;

public sealed class RabbitMqChannelFactory : IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqChannelFactory(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return _channel;
        }

        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
