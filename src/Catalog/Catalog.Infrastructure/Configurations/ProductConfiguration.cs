using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;
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
        builder.Property(p => p.ProductSlug)
            .IsRequired();
        builder.HasIndex(p => p.ProductSlug)
            .IsUnique()
            .HasDatabaseName("UX_Products_ProductSlug");
        builder.HasIndex(p => p.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_Products_Name_trgm");

        builder.HasIndex(p => new { p.CategoryId, p.IsDeleted, p.IsSale, p.IsNew })
            .HasDatabaseName("IX_Products_ListFilters");

        // Cache of the first photo and the scheme URL, empty without them; the links below are the source (R-30).
        builder.Property(p => p.Photo)
            .HasMaxLength(ProductMediaLink.UrlMaxLength)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(p => p.Sсheme)
            .HasMaxLength(ProductMediaLink.UrlMaxLength)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.OwnsMany(p => p.MediaLinks, links =>
        {
            links.ToTable("ProductMedia");
            links.WithOwner().HasForeignKey("ProductId");
            links.Property<ProductId>("ProductId").HasConversion(CatalogConverter.ProductIdConvert);
            links.HasKey("ProductId", nameof(ProductMediaLink.MediaFileId));
            links.Property(link => link.MediaFileId).ValueGeneratedNever();
            links.Property(link => link.Kind).HasConversion<int>().IsRequired();
            links.Property(link => link.Position).IsRequired();
            links.Property(link => link.Url).HasMaxLength(ProductMediaLink.UrlMaxLength).IsRequired();
            // Finds the products of a replaced or deleted library file (R-35).
            links.HasIndex(link => link.MediaFileId).HasDatabaseName("IX_ProductMedia_MediaFileId");
        });
        builder.Navigation(p => p.MediaLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(p => p.Photos);
        builder.Ignore(p => p.Scheme);

        builder.Property(p => p.Stock)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.IsSale)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.IsNew)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.ExportToSite)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.QuantityInPack)
            .HasDefaultValue(0);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
