namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderById;

/// <param name="ProductId">The product id the site sent with the cart line.</param>
/// <param name="Amount">Unit price × quantity.</param>
public sealed record GetAdminQuickOrderItemResponse(string ProductId, string Title, decimal UnitPrice, int Quantity, decimal Amount);
