using Catalog.Domain.Attributes;

namespace Catalog.Infrastructure.Persistence.Attributes;

public sealed class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("CatalogAttributeValues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AttributeIdConverters.ValueId).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.AttributeId).HasConversion(AttributeIdConverters.AttributeId);

        builder.Property(x => x.Code).HasMaxLength(AttributeValue.CodeMaxLength).IsRequired();
        builder.Property(x => x.NameUk).HasMaxLength(AttributeValue.NameMaxLength).IsRequired();
        builder.Property(x => x.NameRu).HasMaxLength(AttributeValue.NameMaxLength);
        builder.Property(x => x.Slug).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SwatchHex).HasMaxLength(7);

        builder.HasOne<CatalogAttribute>().WithMany().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AttributeId, x.Code }).IsUnique()
            .HasDatabaseName("UX_CatalogAttributeValues_Attribute_Code");
        builder.HasIndex(x => new { x.AttributeId, x.Slug }).IsUnique()
            .HasDatabaseName("UX_CatalogAttributeValues_Attribute_Slug");

    }
}
