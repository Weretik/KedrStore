using Catalog.Infrastructure.Persistence.Categories;
using Catalog.Infrastructure.Persistence.Categories.Content;
using Catalog.Infrastructure.Persistence.PriceTypes;
using Catalog.Infrastructure.Persistence.Products;
using Catalog.Infrastructure.Persistence.Products.Prices;
using Catalog.Infrastructure.Persistence.Slugs;
using Catalog.Domain.Groups;

namespace Catalog.Infrastructure.Persistence.Groups;

public sealed class ProductGroupTextConfiguration : IEntityTypeConfiguration<ProductGroupText>
{
    public void Configure(EntityTypeBuilder<ProductGroupText> builder)
    {
        builder.ToTable("CatalogProductGroupTexts");
        builder.HasKey(x => new { x.GroupId, x.Language });
        builder.Property(x => x.GroupId).HasConversion(ProductGroupConfiguration.GroupIdConvert);
        builder.Property(x => x.Language).HasMaxLength(2);
        builder.Property(x => x.ShortDescription).HasMaxLength(ProductGroupText.ShortDescriptionMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(ProductGroupText.DescriptionMaxLength).IsRequired();
        builder.Property(x => x.SeoTitle).HasMaxLength(ProductGroupText.SeoTitleMaxLength).IsRequired();
        builder.Property(x => x.SeoDescription).HasMaxLength(ProductGroupText.SeoDescriptionMaxLength).IsRequired();

        builder.HasOne<ProductGroup>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
