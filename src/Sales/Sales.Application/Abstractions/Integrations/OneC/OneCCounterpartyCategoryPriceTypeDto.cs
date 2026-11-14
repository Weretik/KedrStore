namespace Sales.Application.Abstractions.Integrations.OneC;

public sealed record OneCCounterpartyCategoryPriceTypeDto(
    string CounterpartyId,
    int CategoryId,
    int PriceTypeId);
