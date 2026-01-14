namespace BuildingBlocks.Application.Integrations.OneC.DTOs;

public record OneCPriceDto(int ProductId, int PriceTypeId, decimal Price, string Currency = "UAH");
