using Catalog.Domain.Entities;
using Catalog.Infrastructure.Converters;

namespace Catalog.Infrastructure.Configurations;

public sealed class ProductListProjectionConfiguration : IEntityTypeConfiguration<ProductListProjection>
{
    public void Configure(EntityTypeBuilder<ProductListProjection> builder)
    {
        builder.ToTable("ProductListProjections");

        builder.HasKey(projection => projection.ProductId);

        builder.Property(projection => projection.ProductId)
            .HasConversion(CatalogConverter.ProductIdConvert)
            .ValueGeneratedNever();

        builder.Property(projection => projection.NameUk)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(projection => projection.NameRu)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(projection => projection.ProductSlug)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(projection => projection.Photo)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(projection => projection.CategoryId)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert)
            .IsRequired();

        builder.Property(projection => projection.CategorySlug)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(projection => projection.InStock)
            .IsRequired();

        builder.Property(projection => projection.IsSale)
            .IsRequired();

        builder.Property(projection => projection.IsNew)
            .IsRequired();

        builder.Property(projection => projection.ExportToSite)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(projection => projection.RetailPrice)
            .HasPrecision(18, 2);

        builder.Property(projection => projection.SearchTextUk)
            .HasMaxLength(700)
            .IsRequired();

        builder.Property(projection => projection.SearchTextRu)
            .HasMaxLength(700)
            .IsRequired();

        builder.HasIndex(projection => projection.CategoryId)
            .HasDatabaseName("IX_ProductListProjections_CategoryId");

        builder.HasIndex(projection => projection.CategorySlug)
            .HasDatabaseName("IX_ProductListProjections_CategorySlug");

        builder.HasIndex(projection => new
            {
                projection.CategoryId,
                projection.InStock,
                projection.IsSale,
                projection.IsNew
            })
            .HasDatabaseName("IX_ProductListProjections_ListFilters");

        builder.HasIndex(projection => projection.RetailPrice)
            .HasDatabaseName("IX_ProductListProjections_RetailPrice");

        builder.HasIndex(projection => projection.SearchTextUk)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_ProductListProjections_SearchTextUk_trgm");

        builder.HasIndex(projection => projection.SearchTextRu)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_ProductListProjections_SearchTextRu_trgm");
    }
}
