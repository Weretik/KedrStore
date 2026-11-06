namespace Work.Application.Contracts.Persistence;

/// <summary>Read access for queries; writes go through <see cref="IWorkRepository{T}"/>.</summary>
public interface IReadWorkDbContext
{
    DbSet<WorkTask> WorkTasks { get; }
    DbSet<WorkActivity> WorkActivities { get; }
}
