using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;

namespace Infrastructure.Catalog.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(new ProductId.EfCoreValueConverter())
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .HasConversion(new ProductCategoryId.EfCoreValueConverter());

        builder.Property(p => p.ProductType)
            .HasConversion(
                productType => productType.Name,
                name => ProductType.FromName(name, false)
            )
            .HasMaxLength(50)
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

            price.WithOwner()
                .HasForeignKey("ProductId");

            price.HasKey("ProductId", "PriceType");

            price.Property(p => p.PriceType)
                .HasColumnName("PriceTypeId")
                .IsRequired();

            price.Property(p => p.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property( p => p.CurrencyIso)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            price.HasIndex("ProductId", "PriceTypeId").IsUnique();
        });

        builder.HasIndex(p => new { p.CategoryId, p.ProductType });

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
