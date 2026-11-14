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

public sealed class AttributeAliasMeaningConfiguration : IEntityTypeConfiguration<AttributeAliasMeaning>
{
    public void Configure(EntityTypeBuilder<AttributeAliasMeaning> builder)
    {
        builder.ToTable("CatalogAttributeAliasMeanings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.AliasId).HasConversion(AttributeIdConverters.AliasId);
        builder.Property(x => x.CategoryId).HasConversion(CatalogConverter.ProductCategoryIdConvert);
        builder.Property(x => x.AttributeId).HasConversion(AttributeIdConverters.AttributeId);
        builder.Property(x => x.ValueId).HasConversion(AttributeIdConverters.ValueId);

        builder.HasOne<AttributeAlias>().WithMany().HasForeignKey(x => x.AliasId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CatalogAttribute>().WithMany().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.ValueId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AliasId, x.CategoryId, x.AttributeId }).IsUnique().AreNullsDistinct(false)
            .HasDatabaseName("UX_CatalogAttributeAliasMeanings_Alias_Category_Attribute");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_CatalogAttributeAliasMeanings_CategoryId");
        builder.HasIndex(x => x.AttributeId).HasDatabaseName("IX_CatalogAttributeAliasMeanings_AttributeId");
        builder.HasIndex(x => x.ValueId).HasDatabaseName("IX_CatalogAttributeAliasMeanings_ValueId");
    }
}
