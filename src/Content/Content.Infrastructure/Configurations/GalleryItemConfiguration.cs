
namespace Content.Infrastructure.Configurations;

internal sealed class GalleryItemConfiguration : IEntityTypeConfiguration<GalleryItem>
{
    public void Configure(EntityTypeBuilder<GalleryItem> builder)
    {
        builder.ToTable("GalleryItems", table => table.HasCheckConstraint("CK_GalleryItems_Source",
            $"(\"Kind\" = '{nameof(GalleryItemKind.Photo)}' AND \"MediaFileId\" IS NOT NULL AND \"YoutubeId\" IS NULL) OR " +
            $"(\"Kind\" = '{nameof(GalleryItemKind.Video)}' AND \"YoutubeId\" IS NOT NULL AND \"MediaFileId\" IS NULL)"));
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).UseIdentityByDefaultColumn();
        builder.Property(item => item.Kind).HasConversion<string>().HasMaxLength(16).IsRequired();
        // A media file ID is a value from the Media module, not a foreign key (module rules).
        builder.Property(item => item.MediaFileId);
        builder.Property(item => item.YoutubeId).HasMaxLength(YoutubeVideoId.Length);
        builder.Property(item => item.TitleUk).HasMaxLength(GalleryItemTexts.TitleMaxLength);
        builder.Property(item => item.TitleRu).HasMaxLength(GalleryItemTexts.TitleMaxLength);
        builder.Property(item => item.CaptionUk).HasMaxLength(GalleryItemTexts.CaptionMaxLength);
        builder.Property(item => item.CaptionRu).HasMaxLength(GalleryItemTexts.CaptionMaxLength);
        builder.Property(item => item.CreatedAt).IsRequired();
        builder.Property(item => item.UpdatedAt).IsRequired();

        builder.HasIndex(item => item.Position, "IX_GalleryItems_Position");
        builder.HasIndex(item => item.MediaFileId, "IX_GalleryItems_MediaFileId").IsUnique()
            .HasFilter("\"MediaFileId\" IS NOT NULL");
        builder.HasIndex(item => item.YoutubeId, "IX_GalleryItems_YoutubeId").IsUnique()
            .HasFilter("\"YoutubeId\" IS NOT NULL");
    }
}
