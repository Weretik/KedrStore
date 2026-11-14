namespace Catalog.Application.Features.OneCSync.Source;

public record OneCPriceDto(int ProductId, int PriceTypeId, decimal Price, string Currency = "UAH");
