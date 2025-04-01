using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;

namespace ServiceDiscovery.Nomad;

public class NomadServiceEndpointProviderFactory(ILoggerFactory loggerFactory)
    : IServiceEndpointProviderFactory
{
    public bool TryCreateProvider(ServiceEndpointQuery? query, [NotNullWhen(true)] out IServiceEndpointProvider? provider)
    {
        var logger = loggerFactory.CreateLogger<NomadServiceEndpointProvider>();

        provider = new NomadServiceEndpointProvider(query, logger);

        return true;
    }
}
