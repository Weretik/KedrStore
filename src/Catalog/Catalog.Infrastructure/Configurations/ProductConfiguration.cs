using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Converters;

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

        builder.Property(c => c.ProductTypeIdOneC)
            .HasMaxLength(10)
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

        builder.HasQueryFilter(p => !p.IsDeleted);


    }
}
