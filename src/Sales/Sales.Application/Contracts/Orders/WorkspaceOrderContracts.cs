namespace Sales.Application.Contracts.Orders;

public sealed record WorkspaceCreateOrderRequest(string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines, string QuoteFingerprint, string? Comment);
public sealed record WorkspaceCreatedOrder(long OrderId, string OrderNumber, decimal TotalAmount, string SyncStatus);
/// <param name="AdjustmentAmount">Sum of the manager's discounts (negative) and markups (positive) across all units.</param>
public sealed record WorkspaceOrderListItem(long OrderId, string OrderNumber, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, int LineCount, decimal TotalAmount, string SyncStatus, decimal AdjustmentAmount);
public sealed record WorkspaceOrderDetail(long OrderId, string OrderNumber, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, string? Comment, IReadOnlyList<WorkspaceStoredLine> Lines, decimal TotalAmount, string SyncStatus, string? OneCDocumentNumber);
public sealed record WorkspaceStoredLine(string ProductId, string ProductName, int Quantity, decimal Amount, decimal PriceAdjustment);
