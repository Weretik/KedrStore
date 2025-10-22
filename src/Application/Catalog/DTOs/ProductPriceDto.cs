namespace Application.Catalog.DTOs;

public sealed record ProductPriceDto(string PriceType, decimal Amount, string CurrencyIso);
