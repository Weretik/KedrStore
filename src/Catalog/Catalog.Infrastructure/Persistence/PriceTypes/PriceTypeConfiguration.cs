using Catalog.Infrastructure.Persistence.Categories.Content;
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
using Catalog.Infrastructure.Integrations.OneC;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Categories;

namespace Catalog.Infrastructure.Persistence.PriceTypes;

public class PriceTypeConfiguration : IEntityTypeConfiguration<PriceType>
{
    public void Configure(EntityTypeBuilder<PriceType> builder)
    {
        builder.ToTable("PriceType");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(CatalogConverter.PriceTypeIdConvert)
            .ValueGeneratedNever();

        builder.Property(c => c.PriceTypeName)
            .HasMaxLength(100)
            .IsRequired();
    }
}
