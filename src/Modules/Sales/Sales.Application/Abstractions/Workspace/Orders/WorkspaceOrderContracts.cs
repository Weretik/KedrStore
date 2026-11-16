namespace Sales.Application.Abstractions.Workspace.Orders;

public sealed record WorkspaceCreateOrderRequest(string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines, string QuoteFingerprint, string? Comment);
public sealed record WorkspaceCreatedOrder(long OrderId, string OrderNumber, decimal TotalAmount, string SyncStatus);
/// <param name="AdjustmentAmount">Sum of the manager's discounts (negative) and markups (positive) across all units.</param>
/// <param name="SyncError">Why 1C refused the order or why the last delivery attempt failed (platform spec 006, R-012).</param>
/// <param name="SyncAttempts">Delivery attempts since the order was created or last retried by a manager.</param>
/// <param name="SyncNextAttemptAtUtc">When the outbox sends the order next; null once 1C answered or delivery gave up.</param>
public sealed record WorkspaceOrderListItem(long OrderId, string OrderNumber, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, int LineCount, decimal TotalAmount, string SyncStatus, decimal AdjustmentAmount,
    string? SyncError, int SyncAttempts, DateTimeOffset? SyncLastAttemptAtUtc, DateTimeOffset? SyncNextAttemptAtUtc);
/// <param name="SyncError">See <see cref="WorkspaceOrderListItem"/>.</param>
public sealed record WorkspaceOrderDetail(long OrderId, string OrderNumber, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, string? Comment, IReadOnlyList<WorkspaceStoredLine> Lines, decimal TotalAmount, string SyncStatus, string? OneCDocumentNumber,
    string? SyncError, int SyncAttempts, DateTimeOffset? SyncLastAttemptAtUtc, DateTimeOffset? SyncNextAttemptAtUtc);
public sealed record WorkspaceStoredLine(string ProductId, string ProductName, int Quantity, decimal Amount, decimal PriceAdjustment);
