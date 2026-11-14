namespace Scheduling.Application.Abstractions;

/// <summary>Persistence port of the Scheduling module; implemented by SchedulingDbContext.</summary>
public interface ISchedulingDbContext
{
    DbSet<ScheduledTask> Tasks { get; }
    DbSet<ScheduledTaskRun> Runs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
