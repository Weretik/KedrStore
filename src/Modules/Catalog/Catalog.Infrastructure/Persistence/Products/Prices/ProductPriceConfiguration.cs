using Catalog.Domain.Products;
using Catalog.Domain.Products.Prices;

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
            .HasConversion(id => id.Value, value => ProductPriceId.FromStorage(value))
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ProductId)
            
            .ValueGeneratedNever();

        builder.Property(p => p.PriceTypeId)
            
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

        // Spec catalog/010: set while the owner's price file keeps this price from the 1C sync.
        builder.Property(p => p.LockedFromOneCAt);

        // PostgreSQL xmin: a 1C sync that read a price before the price file locked it fails its save instead of
        // overwriting the file's price (database rules 4).
        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
