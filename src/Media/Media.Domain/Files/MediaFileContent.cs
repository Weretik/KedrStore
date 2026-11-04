namespace Media.Domain.Files;

/// <summary>What is stored on the CDN for a file: its key and what is known about the picture.</summary>
public sealed record MediaFileContent(string StorageKey, string ContentType, long SizeBytes, int? Width, int? Height);
