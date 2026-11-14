using Catalog.Domain.Categories;
using Catalog.Domain.Attributes;

namespace Catalog.Infrastructure.Persistence.Attributes;

public sealed class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.ToTable("CatalogCategoryAttributes");
        builder.HasKey(x => new { x.CategoryId, x.AttributeId });
        builder.Property(x => x.CategoryId).HasConversion(CatalogConverter.ProductCategoryIdConvert).ValueGeneratedNever();
        builder.Property(x => x.AttributeId).HasConversion(AttributeIdConverters.AttributeId).ValueGeneratedNever();

        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CatalogAttribute>().WithMany().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Restrict);
    }
}
