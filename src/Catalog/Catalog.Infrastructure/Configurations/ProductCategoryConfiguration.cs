using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Infrastructure.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

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

        builder.Property(c => c.Id)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert)
            .ValueGeneratedNever();

        builder.Property(c => c.ProductType)
            .HasConversion(CatalogConverter.ProductTypeConvert)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("ProductTypeId")
            .IsRequired();
    }
}
