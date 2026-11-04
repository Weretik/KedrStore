using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Media.Infrastructure.Configurations;

internal sealed class MediaFolderConfiguration : IEntityTypeConfiguration<MediaFolder>
{
    public void Configure(EntityTypeBuilder<MediaFolder> builder)
    {
        builder.ToTable("MediaFolders");
        builder.HasKey(folder => folder.Id);
        builder.Property(folder => folder.Id)
            .HasConversion(id => id.Value, value => MediaFolderId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(folder => folder.ParentId)
            .HasConversion(new ValueConverter<MediaFolderId, long>(id => id.Value, value => MediaFolderId.FromStorage(value)))
            .HasColumnType("bigint");
        builder.Property(folder => folder.Name).HasMaxLength(MediaFolder.NameMaxLength).IsRequired();
        builder.Property(folder => folder.CreatedAt).IsRequired();
        builder.Property(folder => folder.UpdatedAt).IsRequired();

        builder.HasIndex(folder => folder.ParentId);
        builder.HasOne<MediaFolder>().WithMany().HasForeignKey(folder => folder.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}
