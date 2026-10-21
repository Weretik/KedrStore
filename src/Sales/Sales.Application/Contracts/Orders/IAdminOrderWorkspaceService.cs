namespace Sales.Application.Contracts.Orders;

public sealed record WorkspaceInputLine(string ProductId, int Quantity);
public sealed record WorkspaceQuoteRequest(string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines);
public sealed record WorkspaceCreateOrderRequest(string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines, string QuoteFingerprint, string? Comment);
public sealed record WorkspaceQuoteLine(string ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineAmount);
public sealed record WorkspaceQuote(string CounterpartyId, IReadOnlyList<WorkspaceQuoteLine> Lines, decimal TotalAmount, string QuoteFingerprint);
public sealed record WorkspaceCreatedOrder(long OrderId, string OrderNumber, decimal TotalAmount, string SyncStatus);
public sealed record WorkspaceOrderConflict(string Code, WorkspaceQuote? Quote);
public sealed record WorkspaceOrderListItem(long OrderId, string OrderNumber, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, int LineCount, decimal TotalAmount, string SyncStatus);
public sealed record WorkspaceOrderDetail(long OrderId, string OrderNumber, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, string? Comment, IReadOnlyList<WorkspaceStoredLine> Lines, decimal TotalAmount, string SyncStatus, string? OneCDocumentNumber);
public sealed record WorkspaceStoredLine(string ProductId, string ProductName, int Quantity, decimal Amount);
public sealed record WorkspacePagedInfo(int PageNumber, int PageSize, long TotalPages, long TotalRecords);
public sealed record WorkspacePage<T>(WorkspacePagedInfo PagedInfo, IReadOnlyList<T> Value);
public sealed record WorkspaceDraftInput(string Title, string? Note, string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines);
public sealed record WorkspaceDraftListItem(long DraftId, string Title, string CounterpartyId, string CounterpartyName, DateTimeOffset UpdatedAtUtc, int LineCount);
public sealed record WorkspaceDraftQuotedLine(string ProductId, string? ProductName, int Quantity, decimal? UnitPrice, decimal? LineAmount);
public sealed record WorkspaceDraftDetail(long DraftId, string Version, string Title, string? Note, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, IReadOnlyList<WorkspaceDraftQuotedLine> Lines, decimal? TotalAmount, string PricingStatus, string? PricingErrorCode);

public interface IAdminOrderWorkspaceService
{
    Task<WorkspacePage<WorkspaceOrderListItem>> GetOrdersAsync(string? searchTerm, string? counterpartyId, string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<WorkspaceOrderDetail?> GetOrderAsync(long orderId, CancellationToken cancellationToken);
    Task<WorkspaceQuote> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken cancellationToken);
    Task<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)> CreateOrderAsync(WorkspaceCreateOrderRequest request, Guid idempotencyKey, CancellationToken cancellationToken);
    Task<WorkspacePage<WorkspaceDraftListItem>> GetDraftsAsync(string? searchTerm, string? counterpartyId, string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<WorkspaceDraftDetail?> GetDraftAsync(long draftId, CancellationToken cancellationToken);
    Task<(WorkspaceDraftDetail Draft, bool Replay)> CreateDraftAsync(WorkspaceDraftInput request, Guid idempotencyKey, CancellationToken cancellationToken);
    Task<WorkspaceDraftDetail?> UpdateDraftAsync(long draftId, string version, WorkspaceDraftInput request, CancellationToken cancellationToken);
    Task<bool> DeleteDraftAsync(long draftId, string version, CancellationToken cancellationToken);
}
