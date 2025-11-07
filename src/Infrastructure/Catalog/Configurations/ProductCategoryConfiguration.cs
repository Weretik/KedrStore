using Domain.Catalog.Entities;
using Domain.Catalog.ValueObjects;
using Infrastructure.Catalog.Convertors;

namespace Infrastructure.Catalog.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        CategoryMapping.Configure<ProductCategoryId, ProductCategory>(
            builder, "ProductCategories", 100);

        builder.Property(c => c.Id)
            .HasConversion(IdConverter.ProductCategoryIdConvert)
            .ValueGeneratedNever();
    }
}
