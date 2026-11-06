namespace Work.Infrastructure.DataBase;

/// <summary>Staff tasks and the activity log in their own PostgreSQL schema with their own migration history.</summary>
public sealed class WorkDbContext(DbContextOptions<WorkDbContext> options)
    : DbContext(options), IReadWorkDbContext
{
    public const string Schema = "work";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<WorkActivity> WorkActivities => Set<WorkActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkDbContext).Assembly);
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
        => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, Schema));
}
