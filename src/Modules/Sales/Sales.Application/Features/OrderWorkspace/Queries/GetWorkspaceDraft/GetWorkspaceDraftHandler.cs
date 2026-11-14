using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Drafts;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDraft;

public sealed class GetWorkspaceDraftHandler(IWorkspaceDraftReader port)
    : IQueryHandler<GetWorkspaceDraftQuery, Result<WorkspaceDraftDetail>>
{
    public async ValueTask<Result<WorkspaceDraftDetail>> Handle(GetWorkspaceDraftQuery request, CancellationToken cancellationToken)
    {
        var value = await port.GetDraftAsync(request.DraftId, cancellationToken);
        return value is null ? Result.NotFound() : Result.Success(value);
    }
}
