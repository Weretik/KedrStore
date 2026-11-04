using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Media.Infrastructure.Configurations;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(file => file.Id);
        builder.Property(file => file.Id)
            .HasConversion(id => id.Value, value => MediaFileId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(file => file.FolderId)
            .HasConversion(new ValueConverter<MediaFolderId, long>(id => id.Value, value => MediaFolderId.FromStorage(value)))
            .HasColumnType("bigint");
        builder.Property(file => file.FileName).HasMaxLength(MediaFile.FileNameMaxLength).IsRequired();
        builder.Property(file => file.StorageKey).HasMaxLength(MediaFile.StorageKeyMaxLength).IsRequired();
        builder.Property(file => file.ContentType).HasMaxLength(MediaFile.ContentTypeMaxLength).IsRequired();
        builder.Property(file => file.SizeBytes).IsRequired();
        builder.Property(file => file.CreatedAt).IsRequired();
        builder.Property(file => file.UpdatedAt).IsRequired();

        builder.HasIndex(file => file.StorageKey).IsUnique();
        builder.HasIndex(file => new { file.FolderId, file.FileName });
        builder.HasOne<MediaFolder>().WithMany().HasForeignKey(file => file.FolderId).OnDelete(DeleteBehavior.Restrict);
    }
}
