namespace Content.Infrastructure.Persistence.Seo;

/// <summary>The single settings row, guarded by a check constraint; the picture is a media file ID without a key.</summary>
internal sealed class SiteSeoSettingsConfiguration : IEntityTypeConfiguration<SiteSeoSettings>
{
    public void Configure(EntityTypeBuilder<SiteSeoSettings> builder)
    {
        builder.ToTable("SiteSeoSettings", table => table.HasCheckConstraint(
            "CK_SiteSeoSettings_Singleton", $"\"Id\" = {SiteSeoSettings.SingletonId}"));
        builder.HasKey(settings => settings.Id);
        // PostgreSQL xmin: a stale admin save answers 409 (database rules 4).
        builder.Property(settings => settings.Version).IsRowVersion();
        builder.Property(settings => settings.Id)
            .HasConversion(id => id.Value, value => SiteSeoSettingsId.FromStorage(value)).ValueGeneratedNever();
        builder.Property(settings => settings.GoogleAnalyticsId).HasMaxLength(SiteSeoSettings.AnalyticsIdMaxLength);
        builder.Property(settings => settings.GoogleTagManagerId).HasMaxLength(SiteSeoSettings.TagManagerIdMaxLength);
        builder.Property(settings => settings.GoogleSiteVerification)
            .HasMaxLength(SiteSeoSettings.VerificationMaxLength);
        builder.Property(settings => settings.BingSiteVerification).HasMaxLength(SiteSeoSettings.VerificationMaxLength);
        builder.Property(settings => settings.UpdatedAt).IsRequired();
        builder.Property(settings => settings.UpdatedBy).HasMaxLength(EditorName.MaxLength);
    }
}
