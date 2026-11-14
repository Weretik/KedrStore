using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.GetList;

public sealed class GetWorkspaceOrdersQueryHandler(IWorkspaceOrderReader port)
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
