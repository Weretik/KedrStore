namespace Sales.Application.Contracts.Orders;

public sealed record WorkspaceQuoteRequest(string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines);
public sealed record WorkspaceQuoteLine(string ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineAmount);
public sealed record WorkspaceQuote(string CounterpartyId, IReadOnlyList<WorkspaceQuoteLine> Lines, decimal TotalAmount, string QuoteFingerprint);
public sealed record WorkspaceOrderConflict(string Code, WorkspaceQuote? Quote);
