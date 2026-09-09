namespace Sales.Application.Features.Orders.DeliveryFailure;

public sealed record DeadLetterNotification(
    long OrderId,
    string OrderNumber,
    string CounterpartyId,
    string CounterpartyName,
    string? ErrorMessage,
    int AttemptCount,
    DateTimeOffset OccurredAtUtc,
    IReadOnlyList<DeadLetterOrderLine> Lines);

public sealed record DeadLetterOrderLine(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Amount);

public sealed record DeadLetterExcelFile(
    string FileName,
    string ContentType,
    byte[] Bytes);
