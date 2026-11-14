using Catalog.Infrastructure.Persistence.Categories;
using Catalog.Infrastructure.Persistence.Categories.Content;
using Catalog.Infrastructure.Persistence.PriceTypes;
using Catalog.Infrastructure.Persistence.Products;
using Catalog.Infrastructure.Persistence.Products.Prices;
using Catalog.Infrastructure.Persistence.Slugs;
using Catalog.Domain.Attributes;

namespace Catalog.Infrastructure.Persistence.Attributes;

public sealed class AttributeAliasConfiguration : IEntityTypeConfiguration<AttributeAlias>
{
    public void Configure(EntityTypeBuilder<AttributeAlias> builder)
    {
        builder.ToTable("CatalogAttributeAliases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AttributeIdConverters.AliasId).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.AttributeId).HasConversion(AttributeIdConverters.AttributeId);
        builder.Property(x => x.ValueId).HasConversion(AttributeIdConverters.ValueId);

        builder.Property(x => x.Scope).HasMaxLength(AttributeAlias.ScopeMaxLength).IsRequired();
        builder.Property(x => x.RawValue).HasMaxLength(AttributeAlias.RawValueMaxLength).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(AttributeAlias.NoteMaxLength);

        builder.HasOne<CatalogAttribute>().WithMany().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AttributeValue>().WithMany().HasForeignKey(x => x.ValueId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AttributeId, x.Scope, x.RawValue }).IsUnique()
            .HasDatabaseName("UX_CatalogAttributeAliases_Attribute_Scope_Raw");

    }
}
