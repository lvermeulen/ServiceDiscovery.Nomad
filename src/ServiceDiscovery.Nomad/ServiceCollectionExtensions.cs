using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;

namespace ServiceDiscovery.Nomad;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNomadServiceDiscovery(this IServiceCollection services) => services.AddNomadServiceDiscovery(_ => { });

    public static IServiceCollection AddNomadServiceDiscovery(this IServiceCollection services, Action<ServiceDiscoveryOptions> configureOption)
    {
        services
            .AddServiceDiscoveryCore(configureOption)
            .AddSingleton<IServiceEndpointProviderFactory, NomadServiceEndpointProviderFactory>()
            .AddConfigurationServiceEndpointProvider()
            .AddPassThroughServiceEndpointProvider();

        return services;
    }
}
