using Microsoft.Extensions.DependencyInjection;
using ReviewApp.Application.Interfaces;
using ReviewApp.Infrastructure.Seeding;
using ReviewApp.Infrastructure.Services;

namespace ReviewApp.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services
            .AddAzureTableServices()
            .AddDataSeeders();

        return services;
    }

    public static IServiceCollection AddAzureTableServices(this IServiceCollection services)
    {
        services.AddScoped<IReviewService, AzureTableReviewService>();
        services.AddScoped<IProductService, AzureTableProductService>();

        return services;
    }

    public static IServiceCollection AddDataSeeders(this IServiceCollection services)
    {
        services.AddTransient<DataSeeder>();

        return services;
    }
}
