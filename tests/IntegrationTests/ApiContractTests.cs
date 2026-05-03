using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public sealed class ApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task OpenApi_Document_Is_Available()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApi_Contains_Expected_Public_Endpoints()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var paths = root.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/api/catalog/{lang}/products", out var catalogProducts));
        Assert.True(catalogProducts.TryGetProperty("get", out var catalogProductsGet));
        AssertOperationHasPathParameter(catalogProductsGet, "lang");

        Assert.True(paths.TryGetProperty("/api/catalog/{lang}/{categorySlug}/products", out var categoryProducts));
        Assert.True(categoryProducts.TryGetProperty("get", out var categoryProductsGet));
        AssertOperationHasPathParameter(categoryProductsGet, "lang");
        AssertOperationHasPathParameter(categoryProductsGet, "categorySlug");

        Assert.True(paths.TryGetProperty("/api/catalog/{lang}/product/{productSlug}", out var productBySlug));
        Assert.True(productBySlug.TryGetProperty("get", out var productBySlugGet));
        AssertOperationHasPathParameter(productBySlugGet, "lang");
        AssertOperationHasPathParameter(productBySlugGet, "productSlug");

        Assert.True(paths.TryGetProperty("/api/orders", out var orders));
        Assert.True(orders.TryGetProperty("post", out var ordersPost));
        Assert.True(ordersPost.TryGetProperty("requestBody", out _));
    }

    private static void AssertOperationHasPathParameter(JsonElement operation, string name)
    {
        Assert.True(operation.TryGetProperty("parameters", out var parameters));

        var exists = parameters.EnumerateArray().Any(p =>
            p.TryGetProperty("name", out var parameterName) &&
            parameterName.GetString() == name &&
            p.TryGetProperty("in", out var parameterIn) &&
            parameterIn.GetString() == "path");

        Assert.True(exists, $"Expected path parameter '{name}' was not found.");
    }
}
