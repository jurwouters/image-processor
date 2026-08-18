using ImageProcessor.Application.Repositories;
using ImageProcessor.Infrastructure.Data;
using ImageProcessor.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageProcessor.Infrastructure.Extensions;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("ImageProcessor.Infrastructure")
                      .MigrationsHistoryTable("__EFMigrationsHistory", "public")));

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();

        return services;
    }
}
