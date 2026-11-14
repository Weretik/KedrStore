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
using System.Text.Json;
using Catalog.Infrastructure.Integrations.OneC;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Categories;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Infrastructure.Persistence.Attributes;

public sealed class ProductAttributeReviewConfiguration : IEntityTypeConfiguration<ProductAttributeReview>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ProductAttributeReview> builder)
    {
        builder.ToTable("CatalogProductAttributeReviews");
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.ProductId).HasConversion(CatalogConverter.ProductIdConvert).ValueGeneratedNever();
        builder.Property(x => x.BaseName).HasMaxLength(ProductAttributeReview.BaseNameMaxLength).IsRequired();
        builder.Property(x => x.State).HasColumnType("smallint");
        builder.Property(x => x.AcceptedBy).HasMaxLength(ProductAttributeReview.AcceptedByMaxLength);

        // A short list read and written as a whole: one jsonb column instead of a child table.
        builder.Property(x => x.Reasons)
            .HasField("_reasons")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnType("jsonb")
            .HasConversion(
                reasons => JsonSerializer.Serialize(reasons, JsonOptions),
                json => JsonSerializer.Deserialize<List<AttributeReviewReason>>(json, JsonOptions) ?? new List<AttributeReviewReason>(),
                new ValueComparer<IReadOnlyList<AttributeReviewReason>>(
                    (left, right) => left!.SequenceEqual(right!),
                    reasons => reasons.Aggregate(0, (hash, reason) => HashCode.Combine(hash, reason)),
                    reasons => reasons.ToList()))
            .IsRequired();

        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.State).HasDatabaseName("IX_CatalogProductAttributeReviews_State");
        // Grouping reads products by category and base name.
        builder.HasIndex(x => x.BaseName).HasDatabaseName("IX_CatalogProductAttributeReviews_BaseName");
    }
}
