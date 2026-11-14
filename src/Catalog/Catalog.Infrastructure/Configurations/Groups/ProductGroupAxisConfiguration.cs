using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Infrastructure.Configurations.Attributes;

namespace Catalog.Infrastructure.Configurations.Groups;

public sealed class ProductGroupAxisConfiguration : IEntityTypeConfiguration<ProductGroupAxis>
{
    public void Configure(EntityTypeBuilder<ProductGroupAxis> builder)
    {
        builder.ToTable("CatalogProductGroupAxes");
        builder.HasKey(x => new { x.GroupId, x.AttributeId });
        builder.Property(x => x.GroupId).HasConversion(ProductGroupConfiguration.GroupIdConvert);
        builder.Property(x => x.AttributeId).HasConversion(AttributeIdConverters.AttributeId);

        builder.HasOne<ProductGroup>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CatalogAttribute>().WithMany().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AttributeId).HasDatabaseName("IX_CatalogProductGroupAxes_AttributeId");
    }
}
