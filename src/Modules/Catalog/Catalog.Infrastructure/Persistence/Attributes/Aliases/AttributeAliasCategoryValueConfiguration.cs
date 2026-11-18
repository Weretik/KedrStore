using Catalog.Domain.Categories;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.Aliases;
namespace Catalog.Infrastructure.Persistence.Attributes.Aliases;

public sealed class AttributeAliasCategoryValueConfiguration : IEntityTypeConfiguration<AttributeAliasCategoryValue>
{
    public void Configure(EntityTypeBuilder<AttributeAliasCategoryValue> builder)
    {
        builder.ToTable("CatalogAttributeAliasCategoryValues");
        builder.HasKey(x => new { x.AliasId, x.CategoryId });
        builder.Property(x => x.AliasId).HasConversion(AttributeIdConverters.AliasId);
        builder.Property(x => x.CategoryId);
        builder.Property(x => x.ValueId).HasConversion(AttributeIdConverters.ValueId);

        // The relationship to AttributeAlias, the root, is configured in AttributeAliasConfiguration.
        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.ValueId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_CatalogAttributeAliasCategoryValues_CategoryId");
        builder.HasIndex(x => x.ValueId).HasDatabaseName("IX_CatalogAttributeAliasCategoryValues_ValueId");
    }
}
