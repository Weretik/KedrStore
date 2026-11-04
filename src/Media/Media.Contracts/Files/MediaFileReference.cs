namespace Media.Contracts.Files;

/// <summary>
/// A library file as other modules see it: its ID, CDN key, display name, public URL, and pixel size when known.
/// </summary>
public sealed record MediaFileReference(
    long Id, string StorageKey, string FileName, string Url, int? Width = null, int? Height = null);
