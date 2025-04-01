using Xunit.Abstractions;

namespace ServiceDiscovery.Nomad.Tests;

public class ServiceNamePartsShould(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("http://localhost:5000", true, true)]
    [InlineData("localhost:5000", false, true)]
    [InlineData("http://localhost", true, false)]
    [InlineData("localhost", false, false)]
    public void ParseServiceName(string serviceName, bool expectProtocol, bool expectPort)
    {
        Assert.True(ServiceNameParts.TryParse(serviceName, out var parts));
        Assert.Equal("localhost", parts.Host);

        if (expectProtocol)
        {
            Assert.NotNull(parts.EndPointName);
        }

        if (expectPort)
        {
            Assert.NotEqual(0, parts.Port);
        }

        testOutputHelper.WriteLine($"Service name: {serviceName}");
        testOutputHelper.WriteLine($"Host: {parts.Host}");
        testOutputHelper.WriteLine($"Protocol: {parts.EndPointName}");
        testOutputHelper.WriteLine($"Port: {parts.Port}");
    }
}