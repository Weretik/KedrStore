namespace Sales.Application.Integrations.OneC.DTOs;

public sealed record OneCCounterpartyDto(
    string CounterpartyId,
    string CounterpartyName,
    string Email,
    string? Phone,
    int DefaultPriceTypeId);
