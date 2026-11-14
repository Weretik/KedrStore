namespace Sales.Api.Endpoints.QuickOrders.PlaceQuickOrder;

/// <summary>The site quick order body; the record names are the OpenAPI schema names the site uses.</summary>
public sealed record CreateOrderRequest(
    string FirstName,
    string Phone,
    IReadOnlyList<CreateOrderLineRequest> Lines);
