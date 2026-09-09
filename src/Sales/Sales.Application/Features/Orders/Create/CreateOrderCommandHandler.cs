using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sales.Application.Contracts.Catalog;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.Create.DTOs;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.Create;

public sealed class CreateOrderCommandHandler(
    IOrderCreationStore orderCreationStore,
    IOrderProductReader orderProductReader,
    IOrderNumberGenerator orderNumberGenerator)
    : ICommandHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private const string Operation = "create-admin-order";

    public async ValueTask<Result<CreateOrderResult>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var requestHash = CalculateRequestHash(command.Request);
        var idempotencyKey = command.IdempotencyKey.ToString("D");
        var existing = await orderCreationStore.FindIdempotencyRecordAsync(Operation, idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal)
                ? Result.Success(new CreateOrderResult(existing.OrderId.Value, existing.OrderNumber, existing.SyncStatus))
                : Result.Conflict();
        }

        if (!await orderCreationStore.CounterpartyExistsAsync(command.Request.CounterpartyId, cancellationToken))
            return Result.NotFound();

        var products = await orderProductReader.GetByIdsAsync(
            command.Request.Lines.Select(line => line.ProductId).Distinct(StringComparer.Ordinal).ToArray(),
            cancellationToken);
        if (products.Count != command.Request.Lines.Select(line => line.ProductId).Distinct(StringComparer.Ordinal).Count())
            return Result.Invalid(new ValidationError("lines", "One or more products do not exist in the local Catalog."));

        var now = DateTimeOffset.UtcNow;
        var orderNumber = await orderNumberGenerator.GenerateAsync(now, cancellationToken);
        var order = Order.Create(
            orderNumber,
            command.Request.CounterpartyId,
            command.Request.Comment,
            command.Request.Lines.Select(line => OrderLine.Create(line.ProductId, products[line.ProductId].Name, line.Quantity, line.Amount)),
            now);

        await orderCreationStore.PersistAsync(
            order,
            orderId => OneCOrderSync.Create(orderId, now),
            orderId => new CreateOrderIdempotencyRecord(
                Operation, idempotencyKey, requestHash, orderId, orderNumber, OneCOrderSyncStatus.Pending, now, now.AddHours(24)),
            cancellationToken);

        return Result.Created(new CreateOrderResult(order.Id.Value, orderNumber, OneCOrderSyncStatus.Pending));
    }

    private static string CalculateRequestHash(CreateOrderRequest request)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            counterpartyId = request.CounterpartyId.Trim(),
            comment = request.Comment?.Trim(),
            lines = request.Lines.Select(line => new { productId = line.ProductId.Trim(), line.Quantity, line.Amount })
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
