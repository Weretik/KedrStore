namespace Catalog.Application.Shared;

public sealed record ProductPriceDto(string PriceType, decimal Amount, string CurrencyIso);
