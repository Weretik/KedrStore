using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Tasks;

namespace Scheduling.Infrastructure.Persistence;

/// <summary>Schedules and runs of the background tasks in their own schema with their own migration history.</summary>
public sealed class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options), ISchedulingDbContext
{
    public const string Schema = "scheduling";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public DbSet<ScheduledTask> Tasks => Set<ScheduledTask>();
    public DbSet<ScheduledTaskRun> Runs => Set<ScheduledTaskRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulingDbContext).Assembly);
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
        => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, Schema));
}
