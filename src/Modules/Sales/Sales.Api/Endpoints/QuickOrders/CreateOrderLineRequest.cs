namespace Sales.Api.Endpoints.QuickOrders;

public sealed record CreateOrderLineRequest(
    string ProductId,
    string Title,
    decimal UnitPrice,
    int Quantity
);
