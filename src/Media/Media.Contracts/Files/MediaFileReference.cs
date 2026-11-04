namespace Media.Contracts.Files;

/// <summary>A library file as other modules see it: its ID, CDN key, display name, and public URL.</summary>
public sealed record MediaFileReference(long Id, string StorageKey, string FileName, string Url);
