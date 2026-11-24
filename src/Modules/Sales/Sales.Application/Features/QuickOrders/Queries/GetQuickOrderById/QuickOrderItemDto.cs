namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;

public sealed record QuickOrderItemDto(string ProductId, string Title, decimal UnitPrice, int Quantity, decimal Amount);
