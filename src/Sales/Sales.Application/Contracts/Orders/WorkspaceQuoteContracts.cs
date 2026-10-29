namespace Sales.Application.Contracts.Orders;

public sealed record WorkspaceQuoteRequest(string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines);
/// <param name="BaseUnitPrice">The customer's price for one unit before the manager's adjustment.</param>
/// <param name="UnitPrice">The final price for one unit: <paramref name="BaseUnitPrice"/> plus <paramref name="PriceAdjustment"/>.</param>
public sealed record WorkspaceQuoteLine(string ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineAmount,
    decimal BaseUnitPrice, decimal PriceAdjustment);
public sealed record WorkspaceQuote(string CounterpartyId, IReadOnlyList<WorkspaceQuoteLine> Lines, decimal TotalAmount, string QuoteFingerprint);
public sealed record WorkspaceOrderConflict(string Code, WorkspaceQuote? Quote);

public sealed record WorkspaceCustomerPricesRequest(string CounterpartyId, IReadOnlyList<string> ProductIds);
/// <param name="UnitPrice">The customer's UAH price for one unit, or null when the product has no such price.</param>
public sealed record WorkspaceCustomerPrice(string ProductId, decimal? UnitPrice);
public sealed record WorkspaceCustomerPrices(string CounterpartyId, IReadOnlyList<WorkspaceCustomerPrice> Prices);
