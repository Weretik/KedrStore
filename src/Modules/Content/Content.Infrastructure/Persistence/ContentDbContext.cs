using BuildingBlocks.Infrastructure.Persistence;
namespace Content.Infrastructure.Persistence;

/// <summary>Site content (articles, company contacts and requisites, legal documents, gallery, home page FAQ and brands, the page «Співпраця», page search texts, site SEO codes, site redirects) in its own PostgreSQL schema with its own migration history.</summary>
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
    public DbSet<WholesalePage> WholesalePages => Set<WholesalePage>();
    public DbSet<PageSeo> PageSeo => Set<PageSeo>();
    public DbSet<SiteSeoSettings> SiteSeoSettings => Set<SiteSeoSettings>();
    public DbSet<SiteRedirect> SiteRedirects => Set<SiteRedirect>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.AddKernelIds();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        // Trigram indexes of the redirect search (database rules: search).
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString)
        => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, Schema));
}
