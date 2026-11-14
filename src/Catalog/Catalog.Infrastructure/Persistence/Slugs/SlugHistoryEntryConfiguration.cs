using Catalog.Infrastructure.Persistence.Categories;
using Catalog.Infrastructure.Persistence.Categories.Content;
using Catalog.Infrastructure.Persistence.PriceTypes;
using Catalog.Infrastructure.Persistence.Products;
using Catalog.Infrastructure.Persistence.Products.Prices;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;

namespace Catalog.Infrastructure.Persistence.Slugs;

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
