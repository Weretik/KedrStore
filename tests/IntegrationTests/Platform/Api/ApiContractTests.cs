using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Contracts.Orders;

namespace IntegrationTests.Platform.Api;

public sealed class ApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IOrderNumberGenerator, TestOrderNumberGenerator>()));
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

        Assert.True(paths.TryGetProperty("/api/catalog/{lang}/categories", out var categoryTree));
        Assert.True(categoryTree.TryGetProperty("get", out var categoryTreeGet));
        AssertOperationHasPathParameter(categoryTreeGet, "lang");

        Assert.True(paths.TryGetProperty("/api/catalog/{lang}/categories/by-slug/{categorySlug}", out var categoryBySlug));
        Assert.True(categoryBySlug.TryGetProperty("get", out var categoryBySlugGet));
        AssertOperationHasPathParameter(categoryBySlugGet, "lang");
        AssertOperationHasPathParameter(categoryBySlugGet, "categorySlug");

        Assert.True(paths.TryGetProperty("/api/categories", out var adminCategories));
        Assert.True(adminCategories.TryGetProperty("get", out _));
        Assert.True(paths.TryGetProperty("/api/categories/{id}", out var adminCategory));
        Assert.True(adminCategory.TryGetProperty("get", out var adminCategoryGet));
        AssertOperationHasPathParameter(adminCategoryGet, "id");

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

        Assert.True(paths.TryGetProperty("/api/admin/products", out var adminProducts));
        Assert.True(adminProducts.TryGetProperty("get", out _));

        Assert.True(paths.TryGetProperty("/api/admin/products/all", out var allAdminProducts));
        Assert.True(allAdminProducts.TryGetProperty("get", out _));

        Assert.True(paths.TryGetProperty("/api/orders", out var orders));
        Assert.True(orders.TryGetProperty("post", out var ordersPost));
        Assert.True(ordersPost.TryGetProperty("requestBody", out _));

        Assert.True(paths.TryGetProperty("/api/admin/orders", out var adminOrders));
        Assert.True(adminOrders.TryGetProperty("get", out var adminOrdersGet));
        AssertOperationHasQueryParameter(adminOrdersGet, "counterpartyId");
        AssertOperationHasQueryParameter(adminOrdersGet, "page");
        AssertOperationHasQueryParameter(adminOrdersGet, "pageSize");
        Assert.True(adminOrders.TryGetProperty("post", out var adminOrdersPost));
        Assert.True(adminOrdersPost.TryGetProperty("requestBody", out _));
        AssertOperationHasHeaderParameter(adminOrdersPost, "Idempotency-Key");

        Assert.True(paths.TryGetProperty("/api/admin/orders/{orderId}", out var adminOrder));
        Assert.True(adminOrder.TryGetProperty("get", out var adminOrderGet));
        AssertOperationHasPathParameter(adminOrderGet, "orderId");
        Assert.True(adminOrderGet.GetProperty("responses").TryGetProperty("404", out _));

        Assert.True(paths.TryGetProperty("/api/admin/customers", out var adminCustomers));
        Assert.True(adminCustomers.TryGetProperty("get", out var adminCustomersGet));
        AssertOperationHasQueryParameter(adminCustomersGet, "page");
        AssertOperationHasQueryParameter(adminCustomersGet, "pageSize");
        Assert.False(adminCustomersGet.GetProperty("responses").TryGetProperty("401", out _));
        Assert.False(adminCustomersGet.GetProperty("responses").TryGetProperty("403", out _));

        Assert.True(paths.TryGetProperty("/api/admin/customers/{counterpartyId}", out var adminCustomer));
        Assert.True(adminCustomer.TryGetProperty("get", out var adminCustomerGet));
        AssertOperationHasPathParameter(adminCustomerGet, "counterpartyId");
        Assert.True(adminCustomerGet.GetProperty("responses").TryGetProperty("404", out _));
        Assert.False(adminCustomerGet.GetProperty("responses").TryGetProperty("401", out _));
        Assert.False(adminCustomerGet.GetProperty("responses").TryGetProperty("403", out _));

        Assert.True(paths.TryGetProperty("/api/admin/orders/{orderId}/sync/retry", out var retryOrderSync));
        Assert.True(retryOrderSync.TryGetProperty("post", out var retryOrderSyncPost));
        Assert.True(retryOrderSyncPost.TryGetProperty("requestBody", out _));
        Assert.True(retryOrderSyncPost.GetProperty("responses").TryGetProperty("202", out _));
        AssertOperationHasPathParameter(retryOrderSyncPost, "orderId");
    }

    private static void AssertOperationHasHeaderParameter(JsonElement operation, string name)
    {
        Assert.True(operation.TryGetProperty("parameters", out var parameters));
        Assert.Contains(parameters.EnumerateArray(), parameter =>
            parameter.GetProperty("name").GetString() == name && parameter.GetProperty("in").GetString() == "header");
    }

    private static void AssertOperationHasQueryParameter(JsonElement operation, string name)
    {
        Assert.True(operation.TryGetProperty("parameters", out var parameters));
        Assert.Contains(parameters.EnumerateArray(), parameter =>
            parameter.GetProperty("name").GetString() == name && parameter.GetProperty("in").GetString() == "query");
    }

    [Fact]
    public async Task OpenApi_AdminProductRow_Contains_ExportToSite_Flag()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var properties = doc.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("AdminProductListRowDto")
            .GetProperty("properties");

        Assert.True(properties.TryGetProperty("exportToSite", out var exportToSite));
        Assert.Equal("boolean", exportToSite.GetProperty("type").GetString());
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

    private sealed class TestOrderNumberGenerator : IOrderNumberGenerator
    {
        public Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken)
            => Task.FromResult("SO-TEST-0001");
    }
}
