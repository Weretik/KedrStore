using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Platform.Api;

/// <summary>
/// Platform spec 005, R-004/R-006: error answers that need no database are <c>application/problem+json</c> with a
/// machine-readable <c>code</c> and the request's <c>traceId</c>. Result mapping itself is covered by
/// <c>ResultToActionResultTests</c> in the unit tests. Production, because only there the host fills empty error
/// answers through status code pages.
/// </summary>
public sealed class ProblemDetailsContractTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string MobileLogin = "/api/auth/mobile/login";

    [Fact]
    public async Task Validation_Answers400WithFieldErrorsAndCodeValidation()
    {
        using var client = Client();
        using var response = await client.PostAsJsonAsync(MobileLogin,
            new { email = "manager@example.com", password = new string('x', 600) });

        using var problem = await Problem(response, HttpStatusCode.BadRequest);
        Assert.Equal("validation", problem.RootElement.GetProperty("code").GetString());
        var messages = problem.RootElement.GetProperty("errors").GetProperty("Request.Password");
        Assert.Equal(problem.RootElement.GetProperty("detail").GetString(), messages[0].GetString());
    }

    [Fact]
    public async Task MalformedBody_Answers400WithCodeInvalidRequest()
    {
        using var client = Client();
        using var content = new StringContent("{", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(MobileLogin, content);

        using var problem = await Problem(response, HttpStatusCode.BadRequest);
        Assert.Equal("invalid_request", problem.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Anonymous_Answers401WithCodeUnauthorized()
    {
        using var client = Client();
        using var response = await client.GetAsync("/api/profile");

        using var problem = await Problem(response, HttpStatusCode.Unauthorized);
        Assert.Equal("unauthorized", problem.RootElement.GetProperty("code").GetString());
    }

    private HttpClient Client() => factory
        .WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("ConnectionStrings:Default", "Host=localhost;Database=validation;Username=x;Password=x"))
        .CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    private static async Task<JsonDocument> Problem(HttpResponseMessage response, HttpStatusCode status)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(status == response.StatusCode, $"{(int)response.StatusCode}: {body}");
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = JsonDocument.Parse(body);
        Assert.Equal((int)status, problem.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("traceId").GetString()));
        return problem;
    }
}
