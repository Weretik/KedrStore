namespace Catalog.Application.Shared;

public sealed record ProductPriceDto(string PriceType = "1", decimal Amount = 0, string CurrencyIso = "UAH");
