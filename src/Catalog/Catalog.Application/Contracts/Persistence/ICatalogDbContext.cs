using Catalog.Domain.Entities.Attributes;

namespace Catalog.Application.Contracts.Persistence;

public interface IReadCatalogDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<ProductTranslation> ProductTranslations { get; }
    DbSet<ProductListProjection> ProductListProjections { get; }
    DbSet<ProductCategory> Categories { get; }
    DbSet<PriceType> PriceTypes { get; }
    DbSet<SlugHistoryEntry> SlugHistory { get; }
    DbSet<CategoryContent> CategoryContents { get; }
    DbSet<CatalogAttribute> Attributes { get; }
    DbSet<AttributeValue> AttributeValues { get; }
    DbSet<AttributeAlias> AttributeAliases { get; }
    DbSet<CategoryAttribute> CategoryAttributes { get; }
    DbSet<ProductAttributeValue> ProductAttributeValues { get; }
    DbSet<ProductAttributeReview> ProductAttributeReviews { get; }
}
