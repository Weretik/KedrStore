using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Infrastructure.Converters;

namespace Catalog.Infrastructure.Configurations.Attributes;

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
