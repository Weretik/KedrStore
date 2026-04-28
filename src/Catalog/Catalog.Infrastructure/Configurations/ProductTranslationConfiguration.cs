using Catalog.Domain.Entities;
using Catalog.Infrastructure.Converters;

namespace Catalog.Infrastructure.Configurations;

public sealed class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ProductId)
            .HasConversion(CatalogConverter.ProductIdConvert)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.Language })
            .IsUnique();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
