using System.Net;
using Host.Api.Security.TrustedProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Platform.Api;

public sealed class TrustedProxyClientIpMiddlewareTests
{
    private const string Secret = "test-proxy-secret";
    private static readonly IPAddress ProxyIp = IPAddress.Parse("172.68.10.1");

    [Fact]
    public async Task Matching_Secret_Restores_Client_Ip_And_Strips_Headers()
    {
        var context = CreateContext(clientIp: "203.0.113.7", secret: Secret);

        var seen = await InvokeAsync(context, Secret);

        Assert.Equal(IPAddress.Parse("203.0.113.7"), seen.RemoteIp);
        Assert.False(seen.HasClientIpHeader);
        Assert.False(seen.HasSecretHeader);
    }

    [Fact]
    public async Task Wrong_Secret_Keeps_Connection_Ip_And_Strips_Headers()
    {
        var context = CreateContext(clientIp: "203.0.113.7", secret: "wrong");

        var seen = await InvokeAsync(context, Secret);

        Assert.Equal(ProxyIp, seen.RemoteIp);
        Assert.False(seen.HasClientIpHeader);
        Assert.False(seen.HasSecretHeader);
    }

    [Fact]
    public async Task Disabled_Proxy_Ignores_Headers()
    {
        var context = CreateContext(clientIp: "203.0.113.7", secret: Secret);

        var seen = await InvokeAsync(context, sharedSecret: null);

        Assert.Equal(ProxyIp, seen.RemoteIp);
        Assert.False(seen.HasClientIpHeader);
    }

    [Fact]
    public async Task Invalid_Client_Ip_Keeps_Connection_Ip()
    {
        var context = CreateContext(clientIp: "not-an-ip", secret: Secret);

        var seen = await InvokeAsync(context, Secret);

        Assert.Equal(ProxyIp, seen.RemoteIp);
    }

    private static DefaultHttpContext CreateContext(string clientIp, string secret)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = ProxyIp;
        context.Request.Headers["X-Kedr-Client-IP"] = clientIp;
        context.Request.Headers["X-Kedr-Proxy-Secret"] = secret;
        return context;
    }

    private static async Task<SeenRequest> InvokeAsync(HttpContext context, string? sharedSecret)
    {
        SeenRequest? seen = null;
        var middleware = new TrustedProxyClientIpMiddleware(
            next: ctx =>
            {
                seen = new SeenRequest(
                    ctx.Connection.RemoteIpAddress,
                    ctx.Request.Headers.ContainsKey("X-Kedr-Client-IP"),
                    ctx.Request.Headers.ContainsKey("X-Kedr-Proxy-Secret"));
                return Task.CompletedTask;
            },
            Options.Create(new TrustedProxyOptions { SharedSecret = sharedSecret }));

        await middleware.InvokeAsync(context);
        return seen!;
    }

    private sealed record SeenRequest(IPAddress? RemoteIp, bool HasClientIpHeader, bool HasSecretHeader);
}
