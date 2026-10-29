namespace Sales.Application.Contracts.Orders;

public sealed record WorkspaceDraftInput(string Title, string? Note, string CounterpartyId, IReadOnlyList<WorkspaceInputLine> Lines);
public sealed record WorkspaceDraftListItem(long DraftId, string Title, string CounterpartyId, string CounterpartyName, DateTimeOffset UpdatedAtUtc, int LineCount);
public sealed record WorkspaceDraftQuotedLine(string ProductId, string? ProductName, int Quantity, decimal? UnitPrice, decimal? LineAmount,
    decimal PriceAdjustment, decimal? BaseUnitPrice);
public sealed record WorkspaceDraftDetail(long DraftId, string Version, string Title, string? Note, string CounterpartyId, string CounterpartyName, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, IReadOnlyList<WorkspaceDraftQuotedLine> Lines, decimal? TotalAmount, string PricingStatus, string? PricingErrorCode);
