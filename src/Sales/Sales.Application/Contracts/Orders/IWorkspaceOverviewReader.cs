namespace Sales.Application.Contracts.Orders;

public interface IWorkspaceOverviewReader
{
    Task<WorkspaceOverview> GetAsync(DateTimeOffset todayFromUtc, CancellationToken cancellationToken);
}
