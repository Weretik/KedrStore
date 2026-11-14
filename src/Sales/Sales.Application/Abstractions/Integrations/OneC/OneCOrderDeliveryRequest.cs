namespace Sales.Application.Abstractions.Integrations.OneC;

public sealed record OneCOrderDeliveryRequest(
    string CounterpartyId,
    string OrderNumber,
    DateTimeOffset CreatedAtUtc,
    string? Comment,
    IReadOnlyList<OneCOrderDeliveryLineDto> Lines);

public sealed record OneCOrderDeliveryLineDto(
    string ProductId,
    int Quantity,
    decimal Amount);
