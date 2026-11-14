namespace Sales.Application.Abstractions.Orders;

public interface IWorkspaceDraftReader
{
    Task<WorkspacePage<WorkspaceDraftListItem>> GetDraftsAsync(string? searchTerm, string? counterpartyId,
        DateTimeOffset? createdFromUtc, DateTimeOffset? createdToUtcExclusive,
        string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<WorkspaceDraftDetail?> GetDraftAsync(long draftId, CancellationToken cancellationToken);
}
