namespace Sales.Application.Integrations.OneC.DTOs;

public sealed record OneCCounterpartyCategoryPriceTypeDto(
    string CounterpartyId,
    int CategoryId,
    int PriceTypeId);
