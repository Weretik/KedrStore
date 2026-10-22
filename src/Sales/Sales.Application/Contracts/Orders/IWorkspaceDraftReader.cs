namespace Sales.Application.Contracts.Orders;

public interface IWorkspaceDraftReader
{
    Task<WorkspacePage<WorkspaceDraftListItem>> GetDraftsAsync(string? searchTerm, string? counterpartyId,
        DateTimeOffset? createdFromUtc, DateTimeOffset? createdToUtcExclusive,
        string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<WorkspaceDraftDetail?> GetDraftAsync(long draftId, CancellationToken cancellationToken);
}
