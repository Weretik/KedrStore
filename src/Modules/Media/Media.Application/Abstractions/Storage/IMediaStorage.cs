namespace Media.Application.Abstractions.Storage;

/// <summary>A file found on the CDN; <see cref="Key"/> is its path from the CDN root without a leading slash.</summary>
public sealed record StoredMediaObject(string Key, long SizeBytes);

/// <summary>The CDN where library files live. Keys are paths from the CDN root.</summary>
public interface IMediaStorage
{
    /// <summary><c>false</c> without CDN credentials (local runs, tests): writes answer 503.</summary>
    bool IsConfigured { get; }

    Task UploadAsync(string key, Stream content, CancellationToken cancellationToken = default);

    /// <summary>Does nothing when the file is already gone.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Every file on the CDN, recursively, except the top-level directories in <paramref name="excludedTopDirectories"/>.</summary>
    Task<IReadOnlyList<StoredMediaObject>> ListAsync(
        IReadOnlyCollection<string> excludedTopDirectories, CancellationToken cancellationToken = default);
}
