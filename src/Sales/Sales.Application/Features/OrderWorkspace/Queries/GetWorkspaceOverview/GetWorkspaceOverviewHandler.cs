using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOverview;

public sealed class GetWorkspaceOverviewHandler(IWorkspaceOverviewReader reader)
    : IQueryHandler<GetWorkspaceOverviewQuery, Result<WorkspaceOverview>>
{
    public async ValueTask<Result<WorkspaceOverview>> Handle(GetWorkspaceOverviewQuery request, CancellationToken cancellationToken)
        => Result.Success(await reader.GetAsync(request.TodayFromUtc, cancellationToken));
}
