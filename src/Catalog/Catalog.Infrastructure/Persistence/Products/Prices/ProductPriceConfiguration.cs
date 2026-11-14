using Catalog.Infrastructure.Persistence.Categories.Content;
using Catalog.Infrastructure.Persistence.PriceTypes;
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

namespace Catalog.Infrastructure.Persistence.Products.Prices;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");

        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new {p.ProductId, p.PriceTypeId})
            .IsUnique();

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ProductId)
            .HasConversion(CatalogConverter.ProductIdConvert)
            .ValueGeneratedNever();

        builder.Property(p => p.PriceTypeId)
            .HasConversion(CatalogConverter.PriceTypeIdConvert)
            .ValueGeneratedNever();

        builder.Property(c => c.ProductTypeIdOneC)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("Amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property( p => p.CurrencyIso)
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
