using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Application;

using UrlShortener.Application.Interfaces.Services;
using UrlShortener.Application.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IUrlShorteningService, UrlShorteningService>();

        return services;
    }
}