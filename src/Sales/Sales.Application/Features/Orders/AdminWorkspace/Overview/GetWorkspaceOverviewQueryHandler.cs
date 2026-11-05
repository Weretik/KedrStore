using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Overview;

public sealed class GetWorkspaceOverviewQueryHandler(IWorkspaceOverviewReader reader)
    : IQueryHandler<GetWorkspaceOverviewQuery, Result<WorkspaceOverview>>
{
    public async ValueTask<Result<WorkspaceOverview>> Handle(GetWorkspaceOverviewQuery request, CancellationToken cancellationToken)
        => Result.Success(await reader.GetAsync(request.TodayFromUtc, cancellationToken));
}
