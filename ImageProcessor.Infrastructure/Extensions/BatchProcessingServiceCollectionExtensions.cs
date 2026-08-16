using ImageProcessor.Application.Services;
using ImageProcessor.Infrastructure.Services.Batch;
using Microsoft.Extensions.DependencyInjection;

namespace ImageProcessor.Infrastructure.Extensions;

public static class BatchProcessingServiceCollectionExtensions
{
    public static IServiceCollection AddBatchProcessing(this IServiceCollection services)
    {
        services.AddScoped<IBatchService, BatchService>();

        return services;
    }
}
