namespace Media.Application.Contracts.Persistence;

/// <summary>Read access for queries; writes go through <see cref="IMediaRepository{T}"/>.</summary>
public interface IReadMediaDbContext
{
    DbSet<MediaFolder> MediaFolders { get; }
    DbSet<MediaFile> MediaFiles { get; }
}
