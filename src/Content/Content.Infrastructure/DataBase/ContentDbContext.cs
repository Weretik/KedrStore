namespace Content.Infrastructure.DataBase;

/// <summary>Site content (articles, company contacts and requisites, legal documents, gallery, home page FAQ and brands) in its own PostgreSQL schema with its own migration history.</summary>
public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options)
    : DbContext(options), IReadContentDbContext
{
    public const string Schema = "content";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCover> ArticleCovers => Set<ArticleCover>();
    public DbSet<CompanyContacts> CompanyContacts => Set<CompanyContacts>();
    public DbSet<CompanyRequisites> CompanyRequisites => Set<CompanyRequisites>();
    public DbSet<CompanyRequisitesChange> CompanyRequisitesChanges => Set<CompanyRequisitesChange>();
    public DbSet<LegalDocumentVersion> LegalDocumentVersions => Set<LegalDocumentVersion>();
    public DbSet<GalleryItem> GalleryItems => Set<GalleryItem>();
    public DbSet<HomeFaqItem> HomeFaqItems => Set<HomeFaqItem>();
    public DbSet<HomeBrand> HomeBrands => Set<HomeBrand>();

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
