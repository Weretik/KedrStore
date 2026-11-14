namespace Sales.Api.Endpoints.QuickOrders.PlaceQuickOrder;

public sealed record CreateOrderLineRequest(
    string ProductId,
    string Title,
    decimal UnitPrice,
    int Quantity
);
