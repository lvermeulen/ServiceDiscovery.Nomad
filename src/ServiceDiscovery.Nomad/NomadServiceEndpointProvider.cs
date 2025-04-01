using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;

namespace ServiceDiscovery.Nomad;

public class NomadServiceEndpointProvider(string serviceName, ILogger<NomadServiceEndpointProvider> logger) : IServiceEndpointProvider, IHostNameFeature
{
    public string HostName => serviceName;

    public NomadServiceEndpointProvider(ServiceEndpointQuery? query, ILogger<NomadServiceEndpointProvider> logger)
        : this(query?.ServiceName ?? "Unknown", logger)
    { }

    public async ValueTask PopulateAsync(IList<ServiceEndpoint> endpoints, CancellationToken cancellationToken)
    {
        await Task.Yield();

        if (string.IsNullOrEmpty(serviceName))
        {
            logger.LogError("No service name provided.");
            return;
        }

        var total = 0;

        if (ServiceNameParts.TryParse(serviceName, out var serviceNameParts))
        {
            // get value from NOMAD_ADDR_* environment variable
            var envVars = Environment.GetEnvironmentVariables().Keys
                .OfType<string>()
                .Where(x => x.StartsWith("NOMAD_ADDR_") && x.EndsWith(serviceNameParts.Host))
                .ToList();

            foreach (var envVar in envVars)
            {
                var address = Environment.GetEnvironmentVariable(envVar);
                if (address is null)
                {
                    logger.LogWarning("No entry found for service '{ServiceName}' ('{HostName}').", serviceName, HostName);
                    return;
                }

                var isEndpoint = ServiceNameParts.TryCreateEndPoint(address, out var endpoint);
                if (isEndpoint)
                {
                    total++;

                    var serviceEndPoint = ServiceEndpoint.Create(endpoint!);
                    serviceEndPoint.Features.Set<IServiceEndpointProvider>(this);
                    serviceEndPoint.Features.Set<IHostNameFeature>(this);
                    endpoints.Add(serviceEndPoint);

                    logger.LogInformation("Found entry for service {ServiceName}:{Address}.", serviceName, address);
                }
            }
        }

        if (total == 0)
        {
            logger.LogWarning("No entry found for service '{ServiceName}' ('{HostName}').", serviceName, HostName);
        }
    }

    public async ValueTask PopulateAsync(IServiceEndpointBuilder endpoints, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(serviceName))
        {
            logger.LogError("No service name provided.");
            return;
        }

        await PopulateAsync(endpoints.Endpoints, cancellationToken);
    }

    public ValueTask DisposeAsync() => default;

    public override string ToString() => "Nomad";
}
