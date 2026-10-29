namespace Sales.Application.Contracts.Orders;

/// <param name="PriceAdjustment">Per-unit discount (negative) or markup (positive) in UAH; 0 keeps the customer price.</param>
public sealed record WorkspaceInputLine(string ProductId, int Quantity, decimal PriceAdjustment = 0m);
