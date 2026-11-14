namespace Sales.Application.Abstractions.Orders;

public interface IWorkspaceDraftWriter
{
    Task<DomainResult<(WorkspaceDraftDetail Draft, bool Replay)>> CreateDraftAsync(WorkspaceDraftInput request, Guid idempotencyKey, CancellationToken cancellationToken);
    Task<DomainResult<WorkspaceDraftDetail>> UpdateDraftAsync(long draftId, string version, WorkspaceDraftInput request, CancellationToken cancellationToken);
    Task<DomainResult> DeleteDraftAsync(long draftId, string version, CancellationToken cancellationToken);
}
