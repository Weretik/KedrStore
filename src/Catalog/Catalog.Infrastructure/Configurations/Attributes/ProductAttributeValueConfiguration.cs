using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Attributes;
using Catalog.Infrastructure.Converters;

namespace Catalog.Infrastructure.Configurations.Attributes;

public sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("CatalogProductAttributeValues");
        builder.HasKey(x => new { x.ProductId, x.AttributeId, x.Position });
        builder.Property(x => x.ProductId).HasConversion(CatalogConverter.ProductIdConvert).ValueGeneratedNever();
        builder.Property(x => x.AttributeId).HasConversion(AttributeIdConverters.AttributeId).ValueGeneratedNever();
        builder.Property(x => x.ValueId).HasConversion(AttributeIdConverters.ValueId);
        builder.Property(x => x.NumberValue).HasPrecision(18, 3);
        builder.Property(x => x.Source).HasColumnType("smallint");
        builder.Property(x => x.AutomaticValueId).HasConversion(AttributeIdConverters.ValueId);
        builder.Property(x => x.AutomaticNumberValue).HasPrecision(18, 3);
        builder.Property(x => x.AutomaticSource).HasColumnType("smallint");
        builder.Ignore(x => x.IsAutomatic);

        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CatalogAttribute>().WithMany().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.ValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.AutomaticValueId).OnDelete(DeleteBehavior.Restrict);

        // Filters and facet counts read by attribute value.
        builder.HasIndex(x => new { x.AttributeId, x.ValueId })
            .HasDatabaseName("IX_CatalogProductAttributeValues_Attribute_Value");
    }
}
