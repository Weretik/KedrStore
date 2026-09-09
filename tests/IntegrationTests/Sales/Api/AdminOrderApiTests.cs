using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Contracts.Catalog;
using Sales.Application.Contracts.Orders;
using Sales.Application.Contracts.Persistence;
using Sales.Application.Features.Orders.GetSyncStatus.DTOs;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace IntegrationTests.Sales.Api;

public sealed class AdminOrderApiTests
{
    [Fact]
    public async Task Create_ReturnsCreatedPendingResponse_WithoutCallingOneC()
    {
        var state = new TestOrderState();
        using var factory = CreateFactory(state);
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/admin/orders", CreateRequest("active"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(42, body.RootElement.GetProperty("orderId").GetInt64());
        Assert.Equal("SO-TEST-0001", body.RootElement.GetProperty("orderNumber").GetString());
        Assert.Equal("Pending", body.RootElement.GetProperty("syncStatus").GetString());
        Assert.Equal(0, state.OneCCallCount);
        Assert.Equal(OneCOrderSyncStatus.Pending, state.LastSynchronization?.Status);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_ForInvalidOrder()
    {
        using var factory = CreateFactory(new TestOrderState());
        using var client = factory.CreateClient();
        var request = """{"counterpartyId":"active","lines":[{"productId":"101","quantity":0,"amount":125.00}]}""";

        using var response = await client.PostAsync("/api/admin/orders", CreateRequestContent(request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("deleted")]
    public async Task Create_ReturnsNotFound_ForUnknownOrDeletedCounterparty(string counterpartyId)
    {
        using var factory = CreateFactory(new TestOrderState());
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/admin/orders", CreateRequest(counterpartyId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReplaysOriginalResult_AndRejectsConflictingIdempotencyKey()
    {
        using var factory = CreateFactory(new TestOrderState());
        using var client = factory.CreateClient();
        var key = Guid.NewGuid();

        using var initial = await client.PostAsync("/api/admin/orders", CreateRequest("active", key));
        using var replay = await client.PostAsync("/api/admin/orders", CreateRequest("active", key));
        using var conflict = await client.PostAsync("/api/admin/orders", CreateRequest("active", key, "different comment"));

        Assert.Equal(HttpStatusCode.Created, initial.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(await initial.Content.ReadAsStringAsync(), await replay.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetSyncStatus_ReturnsPendingStatusWithoutOneCDocumentNumber()
    {
        var state = new TestOrderState();
        state.SyncStatuses[42] = new GetOrderSyncStatusResult(42, "SO-TEST-0001", OneCOrderSyncStatus.Pending, null, null);
        using var factory = CreateFactory(state);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/admin/orders/42/sync-status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Pending", body.RootElement.GetProperty("syncStatus").GetString());
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("oneCDocumentNumber").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("acceptedAtUtc").ValueKind);
        Assert.False(body.RootElement.TryGetProperty("oneCResponseBody", out _));
        Assert.False(body.RootElement.TryGetProperty("lastErrorMessage", out _));
    }

    [Theory]
    [InlineData(OneCOrderSyncStatus.Accepted, "1C-123", true)]
    [InlineData(OneCOrderSyncStatus.BusinessError, null, false)]
    public async Task GetSyncStatus_ReturnsOnlyAllowedDeliveryFields(
        OneCOrderSyncStatus status,
        string? documentNumber,
        bool hasAcceptedAtUtc)
    {
        DateTimeOffset? acceptedAtUtc = hasAcceptedAtUtc
            ? new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero)
            : null;
        var state = new TestOrderState();
        state.SyncStatuses[42] = new GetOrderSyncStatusResult(42, "SO-TEST-0001", status, documentNumber, acceptedAtUtc);
        using var factory = CreateFactory(state);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/admin/orders/42/sync-status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(status.ToString(), body.RootElement.GetProperty("syncStatus").GetString());
        Assert.Equal(documentNumber, body.RootElement.GetProperty("oneCDocumentNumber").GetString());
        Assert.Equal(hasAcceptedAtUtc, body.RootElement.GetProperty("acceptedAtUtc").ValueKind != JsonValueKind.Null);
        Assert.False(body.RootElement.TryGetProperty("oneCResponseBody", out _));
        Assert.False(body.RootElement.TryGetProperty("lastErrorMessage", out _));
    }

    [Fact]
    public async Task GetSyncStatus_ReturnsNotFoundForUnknownOrder()
    {
        using var factory = CreateFactory(new TestOrderState());
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/admin/orders/404/sync-status");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(OneCOrderSyncStatus.BusinessError)]
    [InlineData(OneCOrderSyncStatus.DeadLetter)]
    public async Task RetrySync_SchedulesTerminalFailureForBackgroundDelivery(OneCOrderSyncStatus status)
    {
        var state = new TestOrderState { RetryStatus = status };
        using var factory = CreateFactory(state);
        using var client = factory.CreateClient();
        using var content = CreateRequestContent("""{"reason":"The 1C reference data was corrected"}""");

        using var response = await client.PostAsync("/api/admin/orders/42/sync/retry", content);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("RetryScheduled", body.RootElement.GetProperty("syncStatus").GetString());
        Assert.Equal("The 1C reference data was corrected", state.RetryReason);
        Assert.Equal(OneCOrderSyncStatus.RetryScheduled, state.RetryStatus);
        Assert.Equal(0, state.OneCCallCount);
    }

    [Fact]
    public async Task RetrySync_ReturnsConflictForAcceptedOrder()
    {
        var state = new TestOrderState { RetryStatus = OneCOrderSyncStatus.Accepted };
        using var factory = CreateFactory(state);
        using var client = factory.CreateClient();
        using var content = CreateRequestContent("""{"reason":"Retry requested"}""");

        using var response = await client.PostAsync("/api/admin/orders/42/sync/retry", content);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OneCOrderSyncStatus.Accepted, state.RetryStatus);
    }

    [Fact]
    public async Task RetrySync_ReturnsNotFoundForUnknownOrder()
    {
        using var factory = CreateFactory(new TestOrderState());
        using var client = factory.CreateClient();
        using var content = CreateRequestContent("""{"reason":"Retry requested"}""");

        using var response = await client.PostAsync("/api/admin/orders/404/sync/retry", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RetrySync_ReturnsBadRequestWhenReasonIsEmpty()
    {
        using var factory = CreateFactory(new TestOrderState());
        using var client = factory.CreateClient();
        using var content = CreateRequestContent("""{"reason":" "}""");

        using var response = await client.PostAsync("/api/admin/orders/42/sync/retry", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void OpenApiContract_DeclaresTheTestedOperationAndResponses()
    {
        var contractPath = Path.Combine(FindRepositoryRoot(), "docs", "sdd", "contracts", "sales", "admin-order-one-c-sync.openapi.yaml");
        var contract = File.ReadAllText(contractPath);

        Assert.Contains("/api/admin/orders:", contract, StringComparison.Ordinal);
        Assert.Contains("name: Idempotency-Key", contract, StringComparison.Ordinal);
        Assert.Contains("'201':", contract, StringComparison.Ordinal);
        Assert.Contains("'200':", contract, StringComparison.Ordinal);
        Assert.Contains("'400':", contract, StringComparison.Ordinal);
        Assert.Contains("'404':", contract, StringComparison.Ordinal);
        Assert.Contains("'409':", contract, StringComparison.Ordinal);
        Assert.Contains("syncStatus: { type: string, enum: [Pending]", contract, StringComparison.Ordinal);
        Assert.Contains("/api/admin/orders/{orderId}/sync-status:", contract, StringComparison.Ordinal);
        Assert.Contains("/api/admin/orders/{orderId}/sync/retry:", contract, StringComparison.Ordinal);
        Assert.Contains("RetryOrderSyncRequest:", contract, StringComparison.Ordinal);
        Assert.Contains("'202':", contract, StringComparison.Ordinal);
        Assert.Contains("OrderSyncStatusResponse:", contract, StringComparison.Ordinal);
        Assert.Contains("oneCDocumentNumber: { type: [string, 'null'], maxLength: 128", contract, StringComparison.Ordinal);
    }

    private static WebApplicationFactory<Program> CreateFactory(TestOrderState state)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IOrderCreationStore>();
                services.RemoveAll<IOrderProductReader>();
                services.RemoveAll<IOrderNumberGenerator>();
                services.RemoveAll<IOrderSyncStatusReader>();
                services.RemoveAll<IOrderSyncRetryStore>();
                services.RemoveAll<ISalesOneCWriteClient>();
                services.AddSingleton(state);
                services.AddScoped<IOrderCreationStore, TestOrderStore>();
                services.AddScoped<IOrderProductReader, TestOrderProductReader>();
                services.AddScoped<IOrderNumberGenerator, TestOrderNumberGenerator>();
                services.AddScoped<IOrderSyncStatusReader, TestOrderSyncStatusReader>();
                services.AddScoped<IOrderSyncRetryStore, TestOrderSyncRetryStore>();
                services.AddScoped<ISalesOneCWriteClient, RecordingOneCWriteClient>();
            });
        });

    private static HttpContent CreateRequest(string counterpartyId, Guid? idempotencyKey = null, string? comment = null)
    {
        var content = CreateRequestContent(JsonSerializer.Serialize(new
        {
            counterpartyId,
            comment,
            lines = new[] { new { productId = "101", quantity = 2, amount = 125.00m } }
        }));
        content.Headers.Remove("Idempotency-Key");
        content.Headers.Add("Idempotency-Key", (idempotencyKey ?? Guid.NewGuid()).ToString());
        return content;
    }

    private static StringContent CreateRequestContent(string json)
    {
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return content;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private sealed class TestOrderState
    {
        public Dictionary<string, CreateOrderIdempotencyRecord> IdempotencyRecords { get; } = new(StringComparer.Ordinal);
        public Dictionary<long, GetOrderSyncStatusResult> SyncStatuses { get; } = [];
        public OneCOrderSync? LastSynchronization { get; set; }
        public int OneCCallCount { get; set; }
        public OneCOrderSyncStatus RetryStatus { get; set; } = OneCOrderSyncStatus.BusinessError;
        public string? RetryReason { get; set; }
    }

    private sealed class TestOrderStore(TestOrderState state) : IOrderCreationStore
    {
        public Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken)
            => Task.FromResult(counterpartyId == "active");

        public Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(string operation, string idempotencyKey, CancellationToken cancellationToken)
            => Task.FromResult(state.IdempotencyRecords.GetValueOrDefault(idempotencyKey));

        public Task PersistAsync(
            Order order,
            Func<OrderId, OneCOrderSync> synchronizationFactory,
            Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
            CancellationToken cancellationToken)
        {
            typeof(Order).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)!
                .SetValue(order, OrderId.Create(42));
            state.LastSynchronization = synchronizationFactory(order.Id);
            state.SyncStatuses[order.Id.Value] = new GetOrderSyncStatusResult(
                order.Id.Value,
                order.OrderNumber,
                state.LastSynchronization.Status,
                state.LastSynchronization.OneCDocumentNumber,
                state.LastSynchronization.AcceptedAtUtc);
            var record = idempotencyRecordFactory(order.Id);
            state.IdempotencyRecords[record.IdempotencyKey] = record;
            return Task.CompletedTask;
        }
    }

    private sealed class TestOrderProductReader : IOrderProductReader
    {
        public Task<IReadOnlyDictionary<string, OrderProductSnapshot>> GetByIdsAsync(
            IReadOnlyCollection<string> productIds,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<string, OrderProductSnapshot>>(
                productIds.ToDictionary(productId => productId, productId => new OrderProductSnapshot(productId, "Test product")));
    }

    private sealed class TestOrderNumberGenerator : IOrderNumberGenerator
    {
        public Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken)
            => Task.FromResult("SO-TEST-0001");
    }

    private sealed class TestOrderSyncStatusReader(TestOrderState state) : IOrderSyncStatusReader
    {
        public Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken)
            => Task.FromResult(state.SyncStatuses.GetValueOrDefault(orderId));
    }

    private sealed class TestOrderSyncRetryStore(TestOrderState state) : IOrderSyncRetryStore
    {
        public Task<OrderSyncRetryStoreResult> ScheduleAsync(
            long orderId,
            string reason,
            string requestedBy,
            DateTimeOffset requestedAtUtc,
            CancellationToken cancellationToken)
        {
            if (orderId != 42)
            {
                return Task.FromResult(new OrderSyncRetryStoreResult(
                    OrderSyncRetryStoreOutcome.NotFound,
                    orderId,
                    null,
                    null));
            }

            var previousStatus = state.RetryStatus;
            if (previousStatus is not (OneCOrderSyncStatus.BusinessError or OneCOrderSyncStatus.DeadLetter))
            {
                return Task.FromResult(new OrderSyncRetryStoreResult(
                    OrderSyncRetryStoreOutcome.InvalidStatus,
                    orderId,
                    "SO-TEST-0001",
                    previousStatus));
            }

            state.RetryReason = reason;
            state.RetryStatus = OneCOrderSyncStatus.RetryScheduled;
            return Task.FromResult(new OrderSyncRetryStoreResult(
                OrderSyncRetryStoreOutcome.Scheduled,
                orderId,
                "SO-TEST-0001",
                previousStatus));
        }
    }

    private sealed class RecordingOneCWriteClient(TestOrderState state) : ISalesOneCWriteClient
    {
        public Task<OneCOrderDeliveryResult> SendOrderAsync(
            OneCOrderDeliveryRequest request,
            CancellationToken cancellationToken)
        {
            state.OneCCallCount++;
            return Task.FromResult(OneCOrderDeliveryResult.Accepted("unexpected", null));
        }
    }
}
