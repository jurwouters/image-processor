using ImageProcessor.Application.Services;
using ImageProcessor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace ImageProcessor.Infrastructure.Extensions;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["S3:Endpoint"];
        var accessKey = configuration["S3:AccessKey"];
        var secretKey = configuration["S3:SecretKey"];

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("Missing configuration: S3:Endpoint.");
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new InvalidOperationException("Missing configuration: S3:AccessKey.");
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("Missing configuration: S3:SecretKey.");

        var uri = new Uri(endpoint);
        var useSsl = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

        services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(uri.Host, uri.Port)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSsl)
                .Build());

        services.AddScoped<IUploadUrlService, MinioUploadUrlService>();

        return services;
    }
}
