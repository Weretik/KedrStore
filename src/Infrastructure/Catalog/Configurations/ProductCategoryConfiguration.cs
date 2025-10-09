namespace Infrastructure.Catalog.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
        => CategoryMapping.Configure<ProductCategoryId, ProductCategory>(
            builder, "ProductCategories", 100);
}
