using Microsoft.Extensions.DependencyInjection;

namespace ServiceDiscovery.Nomad.Tests;

public class HostingExtensionsShould
{
    [Fact]
    public void AddNomadServiceEndpointProvider()
    {
        var services = new ServiceCollection()
            .AddNomadServiceDiscovery(_ => { });

        Assert.Contains(services, x => x.ImplementationType == typeof(NomadServiceEndpointProviderFactory));
    }
}
