using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetList;

public sealed class GetWorkspaceDraftsQueryHandler(IWorkspaceDraftReader port)
    : IQueryHandler<GetWorkspaceDraftsQuery, Result<WorkspacePage<WorkspaceDraftListItem>>>
{
    public async ValueTask<Result<WorkspacePage<WorkspaceDraftListItem>>> Handle(GetWorkspaceDraftsQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(await port.GetDraftsAsync(request.SearchTerm, request.CounterpartyId,
            request.CreatedFromUtc, request.CreatedToUtcExclusive,
            request.Sort, request.Page, request.PageSize, cancellationToken));
    }
}
