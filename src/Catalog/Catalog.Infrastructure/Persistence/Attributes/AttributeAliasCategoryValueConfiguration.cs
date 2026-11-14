using Catalog.Infrastructure.Persistence.Categories.Content;
using Catalog.Infrastructure.Persistence.PriceTypes;
using Catalog.Infrastructure.Persistence.Products;
using Catalog.Infrastructure.Persistence.Products.Prices;
using Catalog.Infrastructure.Persistence.Slugs;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Infrastructure.Integrations.OneC;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Categories;

namespace Catalog.Infrastructure.Persistence.Attributes;

public sealed class AttributeAliasCategoryValueConfiguration : IEntityTypeConfiguration<AttributeAliasCategoryValue>
{
    public void Configure(EntityTypeBuilder<AttributeAliasCategoryValue> builder)
    {
        builder.ToTable("CatalogAttributeAliasCategoryValues");
        builder.HasKey(x => new { x.AliasId, x.CategoryId });
        builder.Property(x => x.AliasId).HasConversion(AttributeIdConverters.AliasId);
        builder.Property(x => x.CategoryId).HasConversion(CatalogConverter.ProductCategoryIdConvert);
        builder.Property(x => x.ValueId).HasConversion(AttributeIdConverters.ValueId);

        builder.HasOne<AttributeAlias>().WithMany().HasForeignKey(x => x.AliasId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.ValueId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_CatalogAttributeAliasCategoryValues_CategoryId");
        builder.HasIndex(x => x.ValueId).HasDatabaseName("IX_CatalogAttributeAliasCategoryValues_ValueId");
    }
}
