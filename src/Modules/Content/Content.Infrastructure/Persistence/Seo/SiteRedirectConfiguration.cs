namespace Content.Infrastructure.Persistence.Seo;

/// <summary>
/// Site redirects (spec content/013). The unique old path guards «one redirect per address» against a racing save;
/// trigram indexes serve the admin search on both addresses.
/// </summary>
internal sealed class SiteRedirectConfiguration : IEntityTypeConfiguration<SiteRedirect>
{
    public void Configure(EntityTypeBuilder<SiteRedirect> builder)
    {
        builder.ToTable("SiteRedirects");
        builder.HasKey(redirect => redirect.Id);
        builder.Property(redirect => redirect.Id)
            .HasConversion(id => id.Value, value => SiteRedirectId.FromStorage(value))
            .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        // PostgreSQL xmin: a stale admin save answers 409 (database rules 4).
        builder.Property(redirect => redirect.Version).IsRowVersion();
        builder.Property(redirect => redirect.FromPath).HasMaxLength(SiteRedirectPath.FromMaxLength).IsRequired();
        builder.Property(redirect => redirect.ToUrl).HasMaxLength(SiteRedirectPath.ToMaxLength).IsRequired();
        builder.Property(redirect => redirect.CreatedAt).IsRequired();
        builder.Property(redirect => redirect.UpdatedAt).IsRequired();
        builder.Property(redirect => redirect.UpdatedBy).HasMaxLength(EditorName.MaxLength).IsRequired();
        builder.HasIndex(redirect => redirect.FromPath, "UX_SiteRedirects_FromPath").IsUnique();
        builder.HasIndex(redirect => redirect.FromPath, "IX_SiteRedirects_FromPath_trgm")
            .HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(redirect => redirect.ToUrl, "IX_SiteRedirects_ToUrl_trgm")
            .HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(redirect => redirect.UpdatedAt, "IX_SiteRedirects_UpdatedAt");
    }
}
