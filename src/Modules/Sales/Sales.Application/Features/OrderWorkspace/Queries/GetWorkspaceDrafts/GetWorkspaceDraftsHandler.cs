using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDrafts;

public sealed class GetWorkspaceDraftsHandler(IWorkspaceDraftReader port)
    : IQueryHandler<GetWorkspaceDraftsQuery, Result<WorkspacePage<WorkspaceDraftListItem>>>
{
    public async ValueTask<Result<WorkspacePage<WorkspaceDraftListItem>>> Handle(GetWorkspaceDraftsQuery request, CancellationToken cancellationToken)
    {
        var dates = WorkspaceDateRange.Parse(request.CreatedFrom, request.CreatedTo);
        if (dates.IsFailure)
            return dates.Error!.ToResult<WorkspacePage<WorkspaceDraftListItem>>();
        return Result.Success(await port.GetDraftsAsync(request.SearchTerm, request.CounterpartyId,
            dates.Value.FromUtc, dates.Value.ToUtcExclusive,
            request.Sort, request.Page, request.PageSize, cancellationToken));
    }
}
