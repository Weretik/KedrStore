using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Infrastructure.Configurations;

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

        builder.Property(c => c.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.ProductTypeIdOneC)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasAlternateKey(c => c.Slug);
        builder.HasIndex(c => c.Slug).IsUnique();

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
    }
}
