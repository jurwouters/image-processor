namespace ImageProcessor.Infrastructure.Messaging;

public sealed class RabbitMqSettings
{
    public required string Host { get; init; }
    public int Port { get; init; } = 5672;
    public required string Username { get; init; }
    public required string Password { get; init; }
    public string QueueName { get; init; } = "image-processing";
}
