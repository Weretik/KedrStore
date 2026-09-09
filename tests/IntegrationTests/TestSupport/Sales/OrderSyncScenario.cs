using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sales.Application.Features.Orders.DeliveryFailure;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Customers.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.Integrations.OneC.Options;
using Sales.Infrastructure.Integrations.OneC.Services;

namespace IntegrationTests.TestSupport.Sales;

internal static class OrderSyncScenario
{
    public static SalesDbContext CreateInMemoryContext(string? databaseName = null)
        => new(new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);

    public static SyncOneCOrdersService CreateService(
        SalesDbContext db,
        OneCOrderDeliveryResult result,
        OneCOrderSyncOptions? options = null)
        => CreateService(db, new RecordingWriteClient(result), new NoOpNotifier(), options);

    public static SyncOneCOrdersService CreateService(
        SalesDbContext db,
        RecordingWriteClient client,
        OneCOrderSyncOptions? options = null)
        => CreateService(db, client, new NoOpNotifier(), options);

    public static SyncOneCOrdersService CreateService(
        SalesDbContext db,
        RecordingWriteClient client,
        IDeadLetterNotifier notifier,
        OneCOrderSyncOptions? options = null)
        => new(
            db,
            client,
            new DeadLetterNotificationService(
                db,
                new NoOpExporter(),
                notifier,
                NullLogger<DeadLetterNotificationService>.Instance),
            Options.Create(options ?? new OneCOrderSyncOptions()),
            NullLogger<SyncOneCOrdersService>.Instance);

    public static async Task<OneCOrderSync> SeedOrderAsync(
        SalesDbContext db,
        DateTimeOffset now,
        string suffix = "1")
    {
        var counterparty = Counterparty.Create(
            $"cp-{suffix}",
            Guid.NewGuid(),
            "Test",
            $"test-{suffix}@example.com",
            null,
            1,
            now);
        var order = Order.Create(
            $"SO-{suffix}",
            counterparty.Id,
            null,
            [OrderLine.Create($"p-{suffix}", "Product", 1, 10m)],
            now);
        db.Counterparties.Add(counterparty);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var sync = OneCOrderSync.Create(order.Id, now);
        db.OneCOrderSyncs.Add(sync);
        await db.SaveChangesAsync();
        return sync;
    }
}

internal sealed class RecordingWriteClient(OneCOrderDeliveryResult result) : ISalesOneCWriteClient
{
    private int _callCount;

    public int CallCount => _callCount;
    public OneCOrderDeliveryRequest? LastRequest { get; private set; }

    public Task<OneCOrderDeliveryResult> SendOrderAsync(
        OneCOrderDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);
        LastRequest = request;
        return Task.FromResult(result);
    }
}

internal sealed class NoOpExporter : IOrderDeliveryFailureExporter
{
    public DeadLetterExcelFile Build(DeadLetterNotification notification)
        => new("dead-letter.xlsx", "application/octet-stream", []);
}

internal sealed class NoOpNotifier : IDeadLetterNotifier
{
    public Task SendAsync(
        DeadLetterNotification notification,
        DeadLetterExcelFile attachment,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class RecordingNotifier : IDeadLetterNotifier
{
    public int CallCount { get; private set; }

    public Task SendAsync(
        DeadLetterNotification notification,
        DeadLetterExcelFile attachment,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}
