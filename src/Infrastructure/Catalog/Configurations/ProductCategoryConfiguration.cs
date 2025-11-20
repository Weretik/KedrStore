using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;

namespace Infrastructure.Catalog.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        CategoryMapping.Configure<ProductCategoryId, ProductCategory>(
            builder, "ProductCategories", 100);

        builder.Property(c => c.Id)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert)
            .ValueGeneratedNever();

        builder.Property(c => c.ProductType)
            .HasConversion(CatalogConverter.ProductTypeConvert)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("ProductTypeId")
            .IsRequired();
    }
}
