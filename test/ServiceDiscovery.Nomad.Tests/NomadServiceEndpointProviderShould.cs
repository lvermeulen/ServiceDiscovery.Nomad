using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using Xunit.Abstractions;

namespace ServiceDiscovery.Nomad.Tests;

public class NomadServiceEndpointProviderShould(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("weatherservice")]
    [InlineData("weatherservice:80")]
    [InlineData("http://weatherservice:80")]
    [InlineData("https://weatherservice:443")]
    [InlineData("http://weatherservice")]
    [InlineData("https+http://weatherservice")]
    public async Task PopulateAsync(string? serviceName)
    {
        // setup
        SetEnvironmentVariables(serviceName, "localhost:80", "localhost:443");
        try
        {
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NomadServiceEndpointProvider>();
            var provider = new NomadServiceEndpointProvider(serviceName, logger);

            var endpoints = new List<ServiceEndpoint>();
            await provider.PopulateAsync(endpoints, CancellationToken.None);

            Assert.NotEmpty(endpoints);
            foreach (var serviceEndpoint in endpoints)
            {
                testOutputHelper.WriteLine($"Endpoint: {serviceEndpoint.EndPoint}");
            }
        }
        finally
        {
            // cleanup
            SetEnvironmentVariables(serviceName, null);
        }
    }

    private static void SetEnvironmentVariables(string? serviceName, params string?[]? value)
    {
        var actualServiceName = GetServiceName(serviceName);

        if (value is null || value.Length == 0)
        {
            Environment.SetEnvironmentVariable($"NOMAD_ADDR_http_{actualServiceName}", null);
            Environment.SetEnvironmentVariable($"NOMAD_ADDR_https_{actualServiceName}", null);
            return;
        }

        Environment.SetEnvironmentVariable($"NOMAD_ADDR_http_{actualServiceName}", value[0]);
        Environment.SetEnvironmentVariable($"NOMAD_ADDR_https_{actualServiceName}", value[1]);
    }

    private static string? GetServiceName(string? serviceName)
    {
        if (serviceName is null)
        {
            return null;
        }

        var result = serviceName
            .Replace("https+http://", string.Empty)
            .Replace("http://", string.Empty)
            .Replace("https://", string.Empty);

        var index = result.LastIndexOf(':');

        result = index > 0 ? result[..index] : result;
        return result;
    }
}
