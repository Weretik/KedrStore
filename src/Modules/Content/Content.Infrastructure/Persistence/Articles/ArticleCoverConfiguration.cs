namespace Content.Infrastructure.Persistence.Articles;

internal sealed class ArticleCoverConfiguration : IEntityTypeConfiguration<ArticleCover>
{
    public void Configure(EntityTypeBuilder<ArticleCover> builder)
    {
        builder.ToTable("ArticleCovers");
        builder.HasKey(cover => cover.ArticleId);
        builder.Property(cover => cover.ArticleId)
            .HasConversion(id => id.Value, value => ArticleId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedNever();
        builder.Property(cover => cover.Data).IsRequired();
        builder.Property(cover => cover.ContentType).HasMaxLength(64).IsRequired();
        builder.HasOne<Article>().WithOne()
            .HasForeignKey<ArticleCover>(cover => cover.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
