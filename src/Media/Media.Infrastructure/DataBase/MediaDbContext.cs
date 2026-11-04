namespace Media.Infrastructure.DataBase;

/// <summary>The media library in its own PostgreSQL schema with its own migration history.</summary>
public sealed class MediaDbContext(DbContextOptions<MediaDbContext> options)
    : DbContext(options), IReadMediaDbContext
{
    public const string Schema = "media";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public DbSet<MediaFolder> MediaFolders => Set<MediaFolder>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
        => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, Schema));
}
