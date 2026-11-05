using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Overview;

/// <param name="TodayFromUtc">Start of the viewer's day; orders from then on count as today's.</param>
public sealed record GetWorkspaceOverviewQuery(DateTimeOffset TodayFromUtc) : IQuery<Result<WorkspaceOverview>>;
