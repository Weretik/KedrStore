namespace Infrastructure.Catalog.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.CategoryId);

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
                t.HasCheckConstraint("CK_ProductPrices_Amount_Positive", "\"Amount\" > 0");
            });

            price.WithOwner()
                .HasForeignKey("ProductId");

            price.HasKey("ProductId", "PriceType");

            price.Property(x => x.PriceType)
                .HasColumnName("PriceTypeId")
                .IsRequired();

            price.Property<decimal>("_amount")
                .HasField("_amount")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property<string>("_currencyIso")
                .HasField("_currencyIso")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();
        });

        builder.HasIndex(x => x.CategoryId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
