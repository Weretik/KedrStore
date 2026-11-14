namespace Sales.Application.Abstractions.Workspace.Overview;

public interface IWorkspaceOverviewReader
{
    Task<WorkspaceOverview> GetAsync(DateTimeOffset todayFromUtc, CancellationToken cancellationToken);
}
