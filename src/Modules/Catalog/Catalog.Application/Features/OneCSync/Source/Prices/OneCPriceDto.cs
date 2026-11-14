namespace Catalog.Application.Features.OneCSync.Source.Prices;

public record OneCPriceDto(int ProductId, int PriceTypeId, decimal Price, string Currency = "UAH");
