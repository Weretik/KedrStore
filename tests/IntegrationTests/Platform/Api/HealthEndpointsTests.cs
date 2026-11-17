using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Platform.Api;

/// <summary>
/// `/health` says the process answers (liveness, no dependencies); `/health/ready` also checks PostgreSQL, so an
/// instance without its database is reported as not ready (reliability REL-04).
/// </summary>
public sealed class HealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Nothing listens on port 1: the connection is refused at once.
    private const string UnreachableDatabase = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=2";

    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:Default", UnreachableDatabase));
    }

    [Fact]
    public async Task Liveness_Answers_Without_The_Database()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Readiness_Is_Unavailable_When_The_Database_Is_Unreachable()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }
}
