using System.Net;
using System.Text.Json;
using Content.Application.Abstractions.Integrations;
using Content.Infrastructure.Integrations.Site;
using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Content;

/// <summary>«Оновити сайт зараз»: the admin endpoint and the request it sends to the site.</summary>
public sealed class SiteRefreshTests
{
    private const string Endpoint = "/api/admin/content/site-refresh";

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.OK)]
    [InlineData("Admin", HttpStatusCode.OK)]
    public async Task Refresh_IsForContentManagers(string? role, HttpStatusCode expected)
    {
        using var factory = CreateFactory(SiteRefreshOutcome.Refreshed);
        using var client = role is null ? factory.CreateClient() : factory.CreateClientWithRole(role);

        using var response = await client.PostAsync(Endpoint, null);

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData(SiteRefreshOutcome.Refreshed, HttpStatusCode.OK, "refreshed")]
    [InlineData(SiteRefreshOutcome.NotConfigured, HttpStatusCode.ServiceUnavailable, "notConfigured")]
    [InlineData(SiteRefreshOutcome.SiteUnavailable, HttpStatusCode.BadGateway, "siteUnavailable")]
    public async Task Refresh_AnswersWithTheOutcome(SiteRefreshOutcome outcome, HttpStatusCode code, string status)
    {
        using var factory = CreateFactory(outcome);
        using var client = factory.CreateClientWithRole("Manager");

        using var response = await client.PostAsync(Endpoint, null);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(code, response.StatusCode);
        Assert.Equal(status, json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Refresher_PostsTheTagsWithTheSecret()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var refresher = Refresher(handler, "https://classkedr.com.ua", "s3cret");

        var outcome = await refresher.RefreshAsync(default);

        Assert.Equal(SiteRefreshOutcome.Refreshed, outcome);
        Assert.Equal("https://classkedr.com.ua/api/revalidate", handler.Request!.RequestUri!.ToString());
        Assert.Equal("s3cret", Assert.Single(handler.Request.Headers.GetValues(SiteRevalidationOptions.SecretHeader)));
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal(["content", "catalog"],
            body.RootElement.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
    }

    [Theory]
    [InlineData(null, "s3cret")]
    [InlineData("https://classkedr.com.ua", null)]
    [InlineData("ftp://classkedr.com.ua", "s3cret")]
    public async Task Refresher_WithoutAddressOrSecret_SendsNothing(string? url, string? secret)
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);

        var outcome = await Refresher(handler, url, secret).RefreshAsync(default);

        Assert.Equal(SiteRefreshOutcome.NotConfigured, outcome);
        Assert.Null(handler.Request);
    }

    [Fact]
    public async Task Refresher_ReportsARefusal()
    {
        var outcome = await Refresher(new RecordingHandler(HttpStatusCode.Unauthorized), "https://classkedr.com.ua", "x")
            .RefreshAsync(default);

        Assert.Equal(SiteRefreshOutcome.SiteUnavailable, outcome);
    }

    private static SiteCacheRefresher Refresher(HttpMessageHandler handler, string? url, string? secret)
        => new(new HttpClient(handler), Options.Create(new SiteRevalidationOptions { Url = url, Secret = secret }),
            NullLogger<SiteCacheRefresher>.Instance);

    private static WebApplicationFactory<Program> CreateFactory(SiteRefreshOutcome outcome)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                services.RemoveAll<ISiteCacheRefresher>();
                services.AddSingleton<ISiteCacheRefresher>(new FakeRefresher(outcome));
            });
        });

    private sealed class FakeRefresher(SiteRefreshOutcome outcome) : ISiteCacheRefresher
    {
        public Task<SiteRefreshOutcome> RefreshAsync(CancellationToken cancellationToken) => Task.FromResult(outcome);
    }

    private sealed class RecordingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status);
        }
    }
}
