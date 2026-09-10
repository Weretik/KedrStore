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
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetById.DTOs;
using Sales.Application.Features.Orders.GetList.DTOs;
using Sales.Domain.Orders.Enums;

namespace IntegrationTests.Sales.Api;

public sealed class ManagerOrderReadApiTests
{
    [Fact]
    public async Task GetList_ReturnsCompactPageAndPassesExactFilter()
    {
        var state = CreateState();
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Manager");

        using var response = await client.GetAsync("/api/admin/orders?counterpartyId=cp-1&page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(11, body.RootElement.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64());
        var row = body.RootElement.GetProperty("value")[0];
        Assert.Equal("SO-42", row.GetProperty("orderNumber").GetString());
        Assert.Equal("Accepted", row.GetProperty("syncStatus").GetString());
        Assert.False(row.TryGetProperty("comment", out _));
        Assert.False(row.TryGetProperty("lines", out _));
        Assert.Equal("cp-1", state.LastListRequest?.CounterpartyId);
        Assert.Equal(2, state.LastListRequest?.Page);
        Assert.Equal(5, state.LastListRequest?.PageSize);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("counterpartyId=%20")]
    public async Task GetList_ReturnsBadRequestForInvalidParameters(string query)
    {
        using var factory = CreateFactory(CreateState());
        using var client = CreateAuthenticatedClient(factory, "Admin");

        using var response = await client.GetAsync($"/api/admin/orders?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetList_ReturnsEmptyPageForUnknownCounterparty()
    {
        var state = CreateState();
        state.ReturnEmptyList = true;
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Manager");

        using var response = await client.GetAsync("/api/admin/orders?counterpartyId=missing");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(body.RootElement.GetProperty("value").EnumerateArray());
    }

    [Fact]
    public async Task GetById_ReturnsApprovedDetailAndDoesNotCallOneC()
    {
        var state = CreateState();
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Admin");

        using var response = await client.GetAsync("/api/admin/orders/42");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Manager note", body.RootElement.GetProperty("comment").GetString());
        Assert.Equal(25m, body.RootElement.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(2, body.RootElement.GetProperty("lines")[0].GetProperty("quantity").GetInt32());
        Assert.False(body.RootElement.GetProperty("lines")[0].TryGetProperty("unitPrice", out _));
        Assert.Equal("Accepted", body.RootElement.GetProperty("sync").GetProperty("status").GetString());
        Assert.False(body.RootElement.GetProperty("sync").TryGetProperty("attemptCount", out _));
        Assert.False(body.RootElement.GetProperty("sync").TryGetProperty("oneCResponseBody", out _));
        Assert.Equal(0, state.OneCCallCount);
        Assert.Equal(42, state.LastOrderId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundForUnknownOrder()
    {
        var state = CreateState();
        state.DetailResult = Result.NotFound();
        using var factory = CreateFactory(state);
        using var client = CreateAuthenticatedClient(factory, "Manager");

        using var response = await client.GetAsync("/api/admin/orders/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/orders")]
    [InlineData("/api/admin/orders/42")]
    public async Task ReadRoutes_ReturnUnauthorizedWithoutAuthentication(string route)
    {
        using var factory = CreateFactory(CreateState());
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/orders")]
    [InlineData("/api/admin/orders/42")]
    public async Task ReadRoutes_ReturnForbiddenForAuthenticatedUser(string route)
    {
        using var factory = CreateFactory(CreateState());
        using var client = CreateAuthenticatedClient(factory, "User");

        using var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static TestOrderReadState CreateState()
    {
        var createdAt = new DateTimeOffset(2026, 9, 10, 10, 30, 0, TimeSpan.Zero);
        var row = new OrderListItemDto(
            42,
            "SO-42",
            "cp-1",
            "Customer",
            createdAt,
            1,
            25m,
            OneCOrderSyncStatus.Accepted,
            "1C-42");
        var detail = new OrderDetailDto(
            42,
            "SO-42",
            createdAt,
            new OrderCounterpartyDto("cp-1", "Customer", "+380000000000"),
            "Manager note",
            [new OrderLineDto("p-1", "Product", 2, 25m)],
            25m,
            new OrderSyncDto(OneCOrderSyncStatus.Accepted, "1C-42", createdAt.AddMinutes(1)));
        return new TestOrderReadState
        {
            Page = new PagedResult<List<OrderListItemDto>>(new PagedInfo(2, 5, 3, 11), [row]),
            DetailResult = Result.Success(detail)
        };
    }

    private static WebApplicationFactory<Program> CreateFactory(TestOrderReadState state)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IOrderReadService>();
                services.AddSingleton(state);
                services.AddScoped<IOrderReadService, TestOrderReadService>();
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

    private sealed class TestOrderReadState
    {
        public required PagedResult<List<OrderListItemDto>> Page { get; init; }
        public required Result<OrderDetailDto> DetailResult { get; set; }
        public bool ReturnEmptyList { get; set; }
        public OrderListRequest? LastListRequest { get; set; }
        public long? LastOrderId { get; set; }
        public int OneCCallCount { get; set; }
    }

    private sealed class TestOrderReadService(TestOrderReadState state) : IOrderReadService
    {
        public Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
            OrderListRequest request,
            CancellationToken cancellationToken)
        {
            state.LastListRequest = request;
            if (!state.ReturnEmptyList)
                return Task.FromResult(Result.Success(state.Page));

            var empty = new PagedResult<List<OrderListItemDto>>(
                new PagedInfo(request.Page, request.PageSize, 0, 0),
                []);
            return Task.FromResult(Result.Success(empty));
        }

        public Task<Result<OrderDetailDto>> GetByIdAsync(long orderId, CancellationToken cancellationToken)
        {
            state.LastOrderId = orderId;
            return Task.FromResult(state.DetailResult);
        }
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "ManagerOrderReadTests";
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
