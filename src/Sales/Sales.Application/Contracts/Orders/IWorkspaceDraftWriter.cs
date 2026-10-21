namespace Sales.Application.Contracts.Orders;

public interface IWorkspaceDraftWriter
{
    Task<(WorkspaceDraftDetail Draft, bool Replay)> CreateDraftAsync(WorkspaceDraftInput request, Guid idempotencyKey, CancellationToken cancellationToken);
    Task<WorkspaceDraftDetail?> UpdateDraftAsync(long draftId, string version, WorkspaceDraftInput request, CancellationToken cancellationToken);
    Task<bool> DeleteDraftAsync(long draftId, string version, CancellationToken cancellationToken);
}
