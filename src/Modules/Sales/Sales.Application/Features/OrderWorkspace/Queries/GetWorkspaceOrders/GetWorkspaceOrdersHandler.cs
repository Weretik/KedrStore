using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrders;

public sealed class GetWorkspaceOrdersHandler(IWorkspaceOrderReader port)
    : IQueryHandler<GetWorkspaceOrdersQuery, Result<WorkspacePage<WorkspaceOrderListItem>>>
{
    public async ValueTask<Result<WorkspacePage<WorkspaceOrderListItem>>> Handle(GetWorkspaceOrdersQuery request, CancellationToken cancellationToken)
    {
        var dates = WorkspaceDateRange.Parse(request.CreatedFrom, request.CreatedTo);
        if (dates.IsFailure)
            return dates.Error!.ToResult<WorkspacePage<WorkspaceOrderListItem>>();
        return Result.Success(await port.GetOrdersAsync(request.SearchTerm, request.CounterpartyId,
            dates.Value.FromUtc, dates.Value.ToUtcExclusive,
            request.Sort, request.Page, request.PageSize, cancellationToken));
    }
}
