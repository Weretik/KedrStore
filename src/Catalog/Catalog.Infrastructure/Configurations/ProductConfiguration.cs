using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(CatalogConverter.ProductIdConvert)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert);
        builder.HasIndex(p => p.CategoryId);

        builder.Property(p => p.Photo)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.Sсheme)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.Stock)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.IsSale)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.IsNew)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.QuantityInPack)
            .HasDefaultValue(0);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Prices)
            .HasField("_prices")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.Prices, price =>
        {
            price.ToTable("ProductPrices",t =>
            {
                t.HasCheckConstraint("CK_ProductPrices_Amount_Positive", "\"Amount\" >= 0");
            });

            price.WithOwner().HasForeignKey("ProductId");
            price.HasKey("ProductId", "PriceType");

            price.Property<ProductId>("ProductId")
                .HasConversion(CatalogConverter.ProductIdConvert)
                .HasColumnName("ProductId")
                .IsRequired();

            price.Property(p => p.PriceType)
                .HasConversion(CatalogConverter.PriceTypeConvert)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PriceType")
                .IsRequired();

            price.Property(p => p.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property( p => p.CurrencyIso)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength()
                .IsUnicode(false)
                .IsRequired();
        });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
