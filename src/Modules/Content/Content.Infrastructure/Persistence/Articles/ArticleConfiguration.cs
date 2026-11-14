namespace Content.Infrastructure.Persistence.Articles;

internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(article => article.Id);
        builder.Property(article => article.Id)
            .HasConversion(id => id.Value, value => ArticleId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(article => article.Slug).HasMaxLength(Article.SlugMaxLength).IsRequired();
        builder.HasIndex(article => article.Slug).IsUnique();
        builder.Property(article => article.PublishedOn).IsRequired();
        builder.Property(article => article.IsPublished).IsRequired();
        builder.Property(article => article.CreatedAt).IsRequired();
        builder.Property(article => article.UpdatedAt).IsRequired();
        builder.Property(article => article.UpdatedBy).HasMaxLength(EditorName.MaxLength);
        // A Media library file ID by value only: the module has no references to Media tables.
        builder.Property(article => article.CoverMediaFileId);
        builder.HasIndex(article => new { article.IsPublished, article.PublishedOn });

        builder.OwnsMany(article => article.Translations, text =>
        {
            text.ToTable("ArticleTranslations");
            text.WithOwner().HasForeignKey("ArticleId");
            text.HasKey("ArticleId", nameof(ArticleTranslation.Language));
            text.Property(t => t.Language).HasMaxLength(2).IsFixedLength();
            text.Property(t => t.Title).HasMaxLength(ArticleTranslation.TitleMaxLength).IsRequired();
            text.Property(t => t.Excerpt).HasMaxLength(ArticleTranslation.ExcerptMaxLength).IsRequired();
            text.Property(t => t.BodyHtml).HasMaxLength(ArticleTranslation.BodyHtmlMaxLength).IsRequired();
        });
        builder.Navigation(article => article.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
