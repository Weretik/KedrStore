using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Configurations;

public sealed class SlugHistoryEntryConfiguration : IEntityTypeConfiguration<SlugHistoryEntry>
{
    public void Configure(EntityTypeBuilder<SlugHistoryEntry> builder)
    {
        builder.ToTable("CatalogSlugHistory");

        builder.HasKey(x => new { x.EntityType, x.Slug });

        builder.Property(x => x.EntityType)
            .HasColumnType("smallint");

        builder.Property(x => x.Slug)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .IsRequired();

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("IX_CatalogSlugHistory_EntityType_EntityId");
    }
}
