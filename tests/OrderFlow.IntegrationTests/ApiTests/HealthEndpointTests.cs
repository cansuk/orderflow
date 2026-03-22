using System.Net;
using FluentAssertions;

namespace OrderFlow.IntegrationTests.ApiTests;

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/healthz");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthController_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
