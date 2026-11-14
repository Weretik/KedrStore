using Catalog.Infrastructure.Persistence.Categories.Content;
using Catalog.Infrastructure.Persistence.PriceTypes;
using Catalog.Infrastructure.Persistence.Products;
using Catalog.Infrastructure.Persistence.Products.Prices;
using Catalog.Infrastructure.Persistence.Slugs;
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
using Catalog.Infrastructure.Integrations.OneC;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Categories;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Infrastructure.Persistence.Categories;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.ShortNameUk)
            .HasMaxLength(ProductCategory.ShortNameMaxLength)
            .IsRequired();

        builder.Property(c => c.ShortNameRu)
            .HasMaxLength(ProductCategory.ShortNameMaxLength)
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.Level)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasDatabaseName("UX_ProductCategories_Slug");

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsShownOnSite)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.ProductTypeIdOneC)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.ParentId)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert);

        builder.Property(c => c.Path)
            .HasConversion(PathConverter.Convert)
            .HasColumnType("ltree")
            .IsRequired()
            .Metadata.SetValueComparer(
                new ValueComparer<CategoryPath>(
                    (pathLeft, pathRight) => pathLeft.Value == pathRight.Value,
                    path => StringComparer.Ordinal.GetHashCode(path.Value),
                    path => CategoryPath.From(path.Value))
            );

        builder.HasIndex(c => c.Path)
            .HasMethod("gist")
            .HasDatabaseName($"IX_ProductCategories_Path_gist");

        builder.HasIndex(c => new { c.ParentId, c.SortOrder, c.Id })
            .HasDatabaseName("IX_ProductCategories_ParentId_SortOrder_Id");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
