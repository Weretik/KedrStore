using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;
using Infrastructure.Catalog.Convertors;

namespace Infrastructure.Catalog.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(IdConverter.ProductIdConvert)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .HasConversion(IdConverter.ProductCategoryIdConvert);

        builder.Property(p => p.ProductType)
            .HasConversion(EnumConverter.ProductTypeConvert)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("ProductType")
            .IsRequired();

        builder.Property(p => p.Photo)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.Stock)
            .HasPrecision(10, 2)
            .IsRequired();

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
                .HasConversion(IdConverter.ProductIdConvert)
                .HasColumnName("ProductId")
                .IsRequired();

            price.Property(p => p.PriceType)
                .HasConversion(EnumConverter.PriceTypeConvert)
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

        builder.HasIndex(p => new { p.CategoryId, p.ProductType });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
