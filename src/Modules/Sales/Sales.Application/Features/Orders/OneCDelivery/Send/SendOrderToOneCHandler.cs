using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Domain.Orders;

namespace Sales.Application.Features.Orders.OneCDelivery.Send;

/// <summary>
/// Sends one order to 1C (platform spec 006, R-011): only inside the sending window; the 1C answer (document or refusal)
/// is stored on the order and saved by the dispatch together with the message outcome; a transport error is a failed
/// attempt for the outbox schedule. 1C recognises a repeat by the order number.
/// </summary>
public sealed class SendOrderToOneCHandler(
    IOrderOneCDeliveryStore store,
    ISalesOneCWriteClient oneC,
    OneCSendingWindow window,
    TimeProvider clock,
    ILogger<SendOrderToOneCHandler> logger) : IOutboxMessageHandler<OrderPlaced>
{
    private const int MaximumDiagnosticLength = 1_000;

    public async Task<OutboxHandlingResult> HandleAsync(
        OrderPlaced message,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var order = await store.FindForDeliveryAsync(message.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} is gone (its customer was removed); nothing to send to 1C.", message.OrderId);
            return OutboxHandlingResult.Delivered;
        }

        if (order.OneCDocumentNumber is not null || order.OneCRejection is not null)
            return OutboxHandlingResult.Delivered;

        var nowUtc = clock.GetUtcNow();
        if (window.NextOpeningAfter(nowUtc) is { } opening)
            return OutboxHandlingResult.RetryAt(opening);

        var answer = await oneC.SendOrderAsync(BuildRequest(order), cancellationToken);
        var diagnostic = Clean(answer.Diagnostic);
        switch (answer.Outcome)
        {
            case OneCOrderDeliveryOutcome.Accepted:
                if (order.AcceptByOneC(answer.OneCDocumentId, nowUtc).IsFailure)
                    order.RejectByOneC("1С прийняла замовлення, але не повернула номер документа.", nowUtc);
                logger.LogInformation("Order {OrderNumber} accepted by 1C as {DocumentNumber}.",
                    order.OrderNumber, order.OneCDocumentNumber);
                return OutboxHandlingResult.Delivered;
            case OneCOrderDeliveryOutcome.BusinessError:
                order.RejectByOneC(diagnostic, nowUtc);
                logger.LogWarning("1C refused order {OrderNumber}: {Reason}.", order.OrderNumber, order.OneCRejection);
                return OutboxHandlingResult.Delivered;
            default:
                return OutboxHandlingResult.Failed(diagnostic ?? "1С не відповіла.");
        }
    }

    private static OneCOrderDeliveryRequest BuildRequest(Order order) => new(
        order.CounterpartyId.Value,
        order.OrderNumber,
        order.CreatedAt,
        order.Comment,
        [.. order.Lines.Select(line => new OneCOrderDeliveryLineDto(line.ProductId.ToString(), line.Quantity, line.Amount))]);

    private static string? Clean(string? diagnostic)
    {
        if (string.IsNullOrWhiteSpace(diagnostic))
            return null;
        var text = new string(diagnostic.Where(character => !char.IsControl(character)).ToArray()).Trim();
        return text.Length <= MaximumDiagnosticLength ? text : text[..MaximumDiagnosticLength];
    }
}
