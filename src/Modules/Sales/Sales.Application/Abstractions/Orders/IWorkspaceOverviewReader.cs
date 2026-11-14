namespace Sales.Application.Abstractions.Orders;

public interface IWorkspaceOverviewReader
{
    Task<WorkspaceOverview> GetAsync(DateTimeOffset todayFromUtc, CancellationToken cancellationToken);
}
