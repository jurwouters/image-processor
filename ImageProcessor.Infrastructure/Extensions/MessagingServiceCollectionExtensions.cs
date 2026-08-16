using ImageProcessor.Application.Messaging;
using ImageProcessor.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageProcessor.Infrastructure.Extensions;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IProcessingQueue, RabbitMqProcessingQueue>();
        services.AddSingleton<RabbitMqChannelFactory>();

        return services;
    }
}
