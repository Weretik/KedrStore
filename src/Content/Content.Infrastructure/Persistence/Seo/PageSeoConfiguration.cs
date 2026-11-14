namespace Content.Infrastructure.Persistence.Seo;

/// <summary>One row per edited site page, keyed by its <see cref="SitePages">name</see>.</summary>
internal sealed class PageSeoConfiguration : IEntityTypeConfiguration<PageSeo>
{
    public void Configure(EntityTypeBuilder<PageSeo> builder)
    {
        builder.ToTable("PageSeo");
        builder.HasKey(seo => seo.Page);
        builder.Property(seo => seo.Page).HasMaxLength(SitePages.MaxLength).ValueGeneratedNever();
        builder.Property(seo => seo.TitleUk).HasMaxLength(PageSeo.TitleMaxLength);
        builder.Property(seo => seo.TitleRu).HasMaxLength(PageSeo.TitleMaxLength);
        builder.Property(seo => seo.DescriptionUk).HasMaxLength(PageSeo.DescriptionMaxLength);
        builder.Property(seo => seo.DescriptionRu).HasMaxLength(PageSeo.DescriptionMaxLength);
        builder.Property(seo => seo.UpdatedAt).IsRequired();
        builder.Property(seo => seo.UpdatedBy).HasMaxLength(EditorName.MaxLength);
    }
}
