using Catalog.Domain.Products;
using Catalog.Domain.Groups;
using Catalog.Domain.Groups.Members;

namespace Catalog.Infrastructure.Persistence.Groups;

public sealed class ProductGroupMemberConfiguration : IEntityTypeConfiguration<ProductGroupMember>
{
    public void Configure(EntityTypeBuilder<ProductGroupMember> builder)
    {
        builder.ToTable("CatalogProductGroupMembers");
        // The product is the key: a product belongs to at most one group.
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.ProductId).HasConversion(CatalogConverter.ProductIdConvert).ValueGeneratedNever();
        builder.Property(x => x.GroupId).HasConversion(ProductGroupConfiguration.GroupIdConvert);

        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductGroup>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GroupId).HasDatabaseName("IX_CatalogProductGroupMembers_GroupId");
    }
}
