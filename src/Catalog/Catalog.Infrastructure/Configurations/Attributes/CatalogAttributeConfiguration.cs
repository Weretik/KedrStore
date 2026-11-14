using Catalog.Domain.Attributes;

namespace Catalog.Infrastructure.Configurations.Attributes;

public sealed class CatalogAttributeConfiguration : IEntityTypeConfiguration<CatalogAttribute>
{
    public void Configure(EntityTypeBuilder<CatalogAttribute> builder)
    {
        builder.ToTable("CatalogAttributes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AttributeIdConverters.AttributeId).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();

        builder.Property(x => x.Code).HasMaxLength(CatalogAttribute.CodeMaxLength).IsRequired();
        builder.Property(x => x.NameUk).HasMaxLength(CatalogAttribute.NameMaxLength).IsRequired();
        builder.Property(x => x.NameRu).HasMaxLength(CatalogAttribute.NameMaxLength);
        builder.Property(x => x.Kind).HasColumnType("smallint");
        builder.Property(x => x.Unit).HasMaxLength(CatalogAttribute.UnitMaxLength);
        builder.Property(x => x.Slug).HasMaxLength(80).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_CatalogAttributes_Code");
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_CatalogAttributes_Slug");

    }
}
