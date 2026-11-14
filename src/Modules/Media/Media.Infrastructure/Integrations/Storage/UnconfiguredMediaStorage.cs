using Media.Application.Abstractions.Storage;

namespace Media.Infrastructure.Integrations.Storage;

/// <summary>Used without CDN credentials (local runs, tests): reads work, writes answer 503.</summary>
internal sealed class UnconfiguredMediaStorage : IMediaStorage
{
    public bool IsConfigured => false;

    public Task UploadAsync(string key, Stream content, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Media storage is not configured.");

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Media storage is not configured.");

    public Task<IReadOnlyList<StoredMediaObject>> ListAsync(
        IReadOnlyCollection<string> excludedTopDirectories, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Media storage is not configured.");
}
