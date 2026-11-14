namespace Media.Application.Features.Files.Jobs.SyncCdnMedia;

/// <summary>How the CDN sync job maps CDN directories to library folders (R-8).</summary>
public sealed class MediaCdnSyncOptions
{
    public const string SectionName = "MediaStorage:Sync";

    /// <summary>Top-level CDN directories the job never lists (Identity owns avatars).</summary>
    public string[] ExcludedDirectories { get; init; } = ["avatars"];

    /// <summary>
    /// Top-level CDN directories the library itself writes to: an unknown file there is an orphan
    /// (an upload whose save failed), reported but not registered.
    /// </summary>
    public string[] OwnedDirectories { get; init; } = ["library"];

    /// <summary>Folder names for top-level CDN directories; other directories keep their own name.</summary>
    public Dictionary<string, string> FolderNames { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["products"] = "Товари",
        ["product-scheme"] = "Схеми"
    };

    public int BatchSize { get; init; } = 500;
}
