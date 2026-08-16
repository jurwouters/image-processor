using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new ImageOperationJsonConverter()));

        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}
