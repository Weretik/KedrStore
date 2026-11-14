using Catalog.Domain.Categories;
using Catalog.Domain.Products;
using Catalog.Domain.Groups;

namespace Catalog.Infrastructure.Persistence.Groups;

public sealed class ProductGroupConfiguration : IEntityTypeConfiguration<ProductGroup>
{
    internal static readonly ValueConverter<ProductGroupId, int> GroupIdConvert =
        new(id => id.Value, value => ProductGroupId.FromStorage(value));

    public void Configure(EntityTypeBuilder<ProductGroup> builder)
    {
        builder.ToTable("CatalogProductGroups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(GroupIdConvert).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.CategoryId).HasConversion(CatalogConverter.ProductCategoryIdConvert);
        builder.Property(x => x.Name).HasMaxLength(ProductGroup.NameMaxLength).IsRequired();
        builder.Property(x => x.GroupKey).HasMaxLength(ProductGroup.NameMaxLength);
        builder.Property(x => x.Slug).HasMaxLength(ProductGroup.SlugMaxLength).IsRequired();
        builder.Property(x => x.MainProductId).HasConversion(CatalogConverter.ProductIdConvert);
        builder.Property(x => x.Status).HasConversion<short>().HasDefaultValue(ProductGroupStatus.Proposed);
        builder.Property(x => x.AcceptedBy).HasMaxLength(ProductGroup.AcceptedByMaxLength);

        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.MainProductId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_CatalogProductGroups_Slug");
        // One automatic group per base name in a category; groups a person created have no key.
        builder.HasIndex(x => new { x.CategoryId, x.GroupKey }).IsUnique()
            .HasFilter("\"GroupKey\" IS NOT NULL")
            .HasDatabaseName("UX_CatalogProductGroups_Category_Key");
        builder.HasIndex(x => x.MainProductId).HasDatabaseName("IX_CatalogProductGroups_MainProductId");
    }
}
