using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.Entities.Groups;

namespace Catalog.Infrastructure.DataBase;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options), IReadCatalogDbContext
{
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ProductListProjection> ProductListProjections => Set<ProductListProjection>();
    public DbSet<PriceType> PriceTypes => Set<PriceType>();
    public DbSet<SlugHistoryEntry> SlugHistory => Set<SlugHistoryEntry>();
    public DbSet<CategoryContent> CategoryContents => Set<CategoryContent>();
    public DbSet<CatalogAttribute> Attributes => Set<CatalogAttribute>();
    public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();
    public DbSet<AttributeAlias> AttributeAliases => Set<AttributeAlias>();
    public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductAttributeReview> ProductAttributeReviews => Set<ProductAttributeReview>();
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();
    public DbSet<ProductGroupMember> ProductGroupMembers => Set<ProductGroupMember>();
    public DbSet<ProductGroupAxis> ProductGroupAxes => Set<ProductGroupAxis>();
    public DbSet<ProductGroupText> ProductGroupTexts => Set<ProductGroupText>();
    public void DiscardChanges() => ChangeTracker.Clear();
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(255);
        configurationBuilder.Properties<Uri>().HaveConversion<UriToStringConverter>().HaveMaxLength(2048);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly,
            type => type.Namespace?.StartsWith("Catalog.Infrastructure") ?? false);

    }
}
