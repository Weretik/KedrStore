using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
namespace IntegrationTests.Platform.Errors;

/// <summary>An unhandled exception answers 500 as ProblemDetails with a traceId and without its text (production).</summary>
public sealed class GlobalExceptionHandlerTests
{
    private const string ThrowPath = "/__tests/throw";

    [Fact]
    public async Task UnhandledException_Answers500ProblemDetailsWithTraceIdAndNoExceptionText()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=validation;Username=x;Password=x");
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter, ThrowingEndpointFilter>());
        });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        using var response = await client.GetAsync(ThrowPath);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret-boom", body);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));
        Assert.Equal("unexpected", json.RootElement.GetProperty("code").GetString());
    }

    /// <summary>
    /// Marks requests to the test path with an anonymous endpoint that throws. Routing keeps an endpoint that is
    /// already set, so the exception is thrown by the app's endpoint middleware, inside its real pipeline.
    /// </summary>
    private sealed class ThrowingEndpointFilter : IStartupFilter
    {
        private static readonly Endpoint Throwing = new(
            _ => throw new InvalidOperationException("secret-boom"),
            new EndpointMetadataCollection(new AllowAnonymousAttribute()),
            "Throwing test endpoint");

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((HttpContext context, RequestDelegate nextMiddleware) =>
            {
                if (context.Request.Path == ThrowPath)
                    context.SetEndpoint(Throwing);
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
