namespace Sales.Application.Features.QuickOrders.Messages;

/// <summary>One product of a site quick order, as the customer saw it in the cart.</summary>
public sealed record QuickOrderLine(string ProductId, string Title, decimal UnitPrice, int Quantity);
