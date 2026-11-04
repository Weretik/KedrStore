namespace Content.Infrastructure.Configurations;

internal sealed class LegalDocumentVersionConfiguration : IEntityTypeConfiguration<LegalDocumentVersion>
{
    public void Configure(EntityTypeBuilder<LegalDocumentVersion> builder)
    {
        builder.ToTable("LegalDocumentVersions");
        builder.HasKey(version => version.Id);
        builder.Property(version => version.Id).UseIdentityByDefaultColumn();
        builder.Property(version => version.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(version => version.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(version => version.UpdatedAt).IsRequired();
        builder.Property(version => version.UpdatedBy).HasMaxLength(LegalDocumentVersion.UserNameMaxLength).IsRequired();
        builder.Property(version => version.PublishedBy).HasMaxLength(LegalDocumentVersion.UserNameMaxLength);

        // At most one draft and one published version per kind; numbers never repeat within a kind.
        builder.HasIndex(version => version.Kind, "IX_LegalDocumentVersions_Kind_Draft").IsUnique()
            .HasFilter("\"Status\" = 'Draft'");
        builder.HasIndex(version => version.Kind, "IX_LegalDocumentVersions_Kind_Published").IsUnique()
            .HasFilter("\"Status\" = 'Published'");
        builder.HasIndex(version => new { version.Kind, version.Number }).IsUnique()
            .HasFilter("\"Number\" IS NOT NULL");
        builder.ToTable(table => table.HasCheckConstraint("CK_LegalDocumentVersions_Published",
            "\"Status\" = 'Draft' OR (\"Number\" IS NOT NULL AND \"EffectiveFrom\" IS NOT NULL AND \"PublishedAt\" IS NOT NULL)"));

        builder.OwnsMany(version => version.Texts, text =>
        {
            text.ToTable("LegalDocumentTexts");
            text.WithOwner().HasForeignKey("VersionId");
            text.HasKey("VersionId", nameof(LegalDocumentText.Language));
            text.Property(t => t.Language).HasMaxLength(2).IsFixedLength();
            text.Property(t => t.Title).HasMaxLength(LegalDocumentText.TitleMaxLength).IsRequired();
            text.Property(t => t.BodyHtml).HasMaxLength(LegalDocumentText.BodyHtmlMaxLength).IsRequired();
        });
        builder.Navigation(version => version.Texts).HasField("_texts").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
