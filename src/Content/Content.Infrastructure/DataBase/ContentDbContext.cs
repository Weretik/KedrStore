namespace Content.Infrastructure.DataBase;

/// <summary>Site content (articles, company contacts) in its own PostgreSQL schema with its own migration history.</summary>
public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options)
    : DbContext(options), IReadContentDbContext
{
    public const string Schema = "content";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCover> ArticleCovers => Set<ArticleCover>();
    public DbSet<CompanyContacts> CompanyContacts => Set<CompanyContacts>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
        => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, Schema));
}
