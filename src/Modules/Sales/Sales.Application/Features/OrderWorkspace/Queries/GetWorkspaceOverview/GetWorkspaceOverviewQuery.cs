using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOverview;

/// <param name="TodayFromUtc">Start of the viewer's day; orders from then on count as today's.</param>
public sealed record GetWorkspaceOverviewQuery(DateTimeOffset TodayFromUtc) : IQuery<Result<WorkspaceOverview>>;
