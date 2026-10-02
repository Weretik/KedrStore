using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Infrastructure.Converters;

namespace Catalog.Infrastructure.Configurations;

public sealed class ProductPhotoCheckConfiguration : IEntityTypeConfiguration<ProductPhotoCheck>
{
    public void Configure(EntityTypeBuilder<ProductPhotoCheck> builder)
    {
        builder.ToTable("ProductPhotoChecks", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ProductPhotoChecks_Status",
                "\"Status\" IN ('Unknown', 'Available', 'Missing', 'InvalidContentType', 'CheckFailed')");
            tableBuilder.HasCheckConstraint(
                "CK_ProductPhotoChecks_State",
                """
                ("Status" = 'Unknown' AND "CheckedAtUtc" IS NULL AND "HttpStatusCode" IS NULL AND "DiagnosticCode" IS NULL AND "DiagnosticMessage" IS NULL)
                OR ("Status" = 'Available' AND "CheckedAtUtc" IS NOT NULL AND "HttpStatusCode" BETWEEN 200 AND 299 AND "DiagnosticCode" IS NULL AND "DiagnosticMessage" IS NULL)
                OR ("Status" = 'Missing' AND "CheckedAtUtc" IS NOT NULL AND "HttpStatusCode" IN (404, 410) AND "DiagnosticCode" IS NOT NULL)
                OR ("Status" = 'InvalidContentType' AND "CheckedAtUtc" IS NOT NULL AND "HttpStatusCode" BETWEEN 200 AND 299 AND "DiagnosticCode" IS NOT NULL)
                OR ("Status" = 'CheckFailed' AND "CheckedAtUtc" IS NOT NULL AND ("HttpStatusCode" IS NULL OR "HttpStatusCode" BETWEEN 100 AND 599) AND "DiagnosticCode" IS NOT NULL)
                """);
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(CatalogConverter.ProductIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(ProductPhotoCheck.PhotoUrlMaxLength)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CheckedAtUtc);

        builder.Property(x => x.HttpStatusCode);

        builder.Property(x => x.DiagnosticCode)
            .HasMaxLength(ProductPhotoCheck.DiagnosticCodeMaxLength);

        builder.Property(x => x.DiagnosticMessage)
            .HasMaxLength(ProductPhotoCheck.DiagnosticMessageMaxLength);

        builder.HasIndex(x => new { x.Status, x.Id })
            .HasDatabaseName("IX_ProductPhotoChecks_Status_ProductId");

        builder.HasOne<Product>()
            .WithOne()
            .HasForeignKey<ProductPhotoCheck>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
