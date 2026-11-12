namespace Content.Infrastructure.Configurations;

/// <summary>The single settings row, guarded by a check constraint; the picture is a media file ID without a key.</summary>
internal sealed class SiteSeoSettingsConfiguration : IEntityTypeConfiguration<SiteSeoSettings>
{
    public void Configure(EntityTypeBuilder<SiteSeoSettings> builder)
    {
        builder.ToTable("SiteSeoSettings", table => table.HasCheckConstraint(
            "CK_SiteSeoSettings_Singleton", $"\"Id\" = {SiteSeoSettings.SingletonId}"));
        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.Id).ValueGeneratedNever();
        builder.Property(settings => settings.GoogleAnalyticsId).HasMaxLength(SiteSeoSettings.AnalyticsIdMaxLength);
        builder.Property(settings => settings.GoogleTagManagerId).HasMaxLength(SiteSeoSettings.TagManagerIdMaxLength);
        builder.Property(settings => settings.GoogleSiteVerification)
            .HasMaxLength(SiteSeoSettings.VerificationMaxLength);
        builder.Property(settings => settings.BingSiteVerification).HasMaxLength(SiteSeoSettings.VerificationMaxLength);
        builder.Property(settings => settings.UpdatedAt).IsRequired();
        builder.Property(settings => settings.UpdatedBy).HasMaxLength(EditorName.MaxLength);
    }
}
