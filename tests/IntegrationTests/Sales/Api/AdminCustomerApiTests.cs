using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace IntegrationTests.Sales.Api;

public sealed class AdminCustomerApiTests
{
    [Fact]
    public async Task GetList_ReturnsCompactPageAndUsesPagingDefaults()
    {
        var state = CreateState();
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Manager");

        using var response = await client.GetAsync("/api/admin/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = body.RootElement.GetProperty("value")[0];
        Assert.Equal("customer-1", row.GetProperty("counterpartyId").GetString());
        Assert.False(row.TryGetProperty("email", out _));
        Assert.False(row.TryGetProperty("defaultPriceTypeId", out _));
        Assert.Equal(1, state.LastListRequest?.Page);
        Assert.Equal(20, state.LastListRequest?.PageSize);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task GetList_ReturnsBadRequestForInvalidPaging(string query)
    {
        using var factory = CreateFactory(CreateState());
        using var client = CreateAuthenticatedClient(factory, "Admin");

        using var response = await client.GetAsync($"/api/admin/customers?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetList_ReturnsEmptyPage()
    {
        var state = CreateState();
        state.ReturnEmptyList = true;
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Manager");

        using var response = await client.GetAsync("/api/admin/customers?page=3&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(body.RootElement.GetProperty("value").EnumerateArray());
    }

    [Fact]
    public async Task GetById_ReturnsApprovedDetailWithoutPrivateOrOrderData()
    {
        var state = CreateState();
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Admin");

        using var response = await client.GetAsync("/api/admin/customers/customer%2D1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("customer-1", body.RootElement.GetProperty("counterpartyId").GetString());
        Assert.Equal(10, body.RootElement.GetProperty("categoryPriceTypes")[0].GetProperty("categoryId").GetInt32());
        Assert.False(body.RootElement.TryGetProperty("identityUserId", out _));
        Assert.False(body.RootElement.TryGetProperty("orders", out _));
        Assert.False(body.RootElement.TryGetProperty("isDeleted", out _));
        Assert.Equal("customer-1", state.LastCounterpartyId);
        Assert.Equal(0, state.ExternalCallCount);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("deleted")]
    public async Task GetById_ReturnsNotFoundForUnavailableCustomer(string counterpartyId)
    {
        var state = CreateState();
        state.DetailResult = Result.NotFound();
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Manager");

        using var response = await client.GetAsync($"/api/admin/customers/{counterpartyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsBadRequestForInvalidIdentifier()
    {
        using var factory = CreateFactory(CreateState());
        using var client = CreateAuthenticatedClient(factory, "Admin");

        using var response = await client.GetAsync($"/api/admin/customers/{new string('x', 65)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/customers")]
    [InlineData("/api/admin/customers/customer-1")]
    public async Task ReadRoutes_ReturnSuccessWithoutAuthentication(string route)
    {
        using var factory = CreateFactory(CreateState());
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static TestCustomerReadState CreateState()
    {
        var detail = new CustomerDetailDto(
            "customer-1",
            "Customer",
            null,
            "customer@example.com",
            1,
            [new CustomerCategoryPriceTypeDto(10, 20)]);
        return new TestCustomerReadState
        {
            Page = new PagedResult<List<CustomerListItemDto>>(
                new PagedInfo(1, 20, 1, 1),
                [new CustomerListItemDto("customer-1", "Customer", null)]),
            DetailResult = Result.Success(detail)
        };
    }

    private static WebApplicationFactory<Program> CreateFactory(TestCustomerReadState state)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICustomerReadService>();
                services.AddSingleton(state);
                services.AddScoped<ICustomerReadService, TestCustomerReadService>();
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                        options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName,
                        _ => { });
            });
        });

    private static HttpClient CreateAuthenticatedClient(WebApplicationFactory<Program> factory, string role)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, role);
        return client;
    }

    private sealed class TestCustomerReadState
    {
        public required PagedResult<List<CustomerListItemDto>> Page { get; init; }
        public required Result<CustomerDetailDto> DetailResult { get; set; }
        public bool ReturnEmptyList { get; set; }
        public CustomerListRequest? LastListRequest { get; set; }
        public string? LastCounterpartyId { get; set; }
        public int ExternalCallCount { get; set; }
    }

    private sealed class TestCustomerReadService(TestCustomerReadState state) : ICustomerReadService
    {
        public Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
            CustomerListRequest request,
            CancellationToken cancellationToken)
        {
            state.LastListRequest = request;
            if (!state.ReturnEmptyList)
                return Task.FromResult(Result.Success(state.Page));

            return Task.FromResult(Result.Success(new PagedResult<List<CustomerListItemDto>>(
                new PagedInfo(request.Page, request.PageSize, 0, 0),
                [])));
        }

        public Task<Result<CustomerDetailDto>> GetByIdAsync(
            string counterpartyId,
            CancellationToken cancellationToken)
        {
            state.LastCounterpartyId = counterpartyId;
            return Task.FromResult(state.DetailResult);
        }
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "CustomerReadTests";
        public const string RoleHeader = "X-Test-Role";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(RoleHeader, out var role) || string.IsNullOrWhiteSpace(role))
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "test-user"), new Claim(ClaimTypes.Role, role.ToString())],
                SchemeName);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
