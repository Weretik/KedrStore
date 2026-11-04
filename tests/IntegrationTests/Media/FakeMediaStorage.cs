using System.Collections.Concurrent;
using Media.Application.Contracts.Storage;

namespace IntegrationTests.Media;

/// <summary>An in-memory CDN: keys to bytes.</summary>
internal sealed class FakeMediaStorage(bool isConfigured = true) : IMediaStorage
{
    public ConcurrentDictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

    public bool IsConfigured { get; } = isConfigured;

    public async Task UploadAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        Files[key] = buffer.ToArray();
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        Files.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoredMediaObject>> ListAsync(
        IReadOnlyCollection<string> excludedTopDirectories, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StoredMediaObject>>(Files
            .Where(file => !excludedTopDirectories.Any(directory =>
                file.Key.StartsWith(directory + "/", StringComparison.OrdinalIgnoreCase)))
            .Select(file => new StoredMediaObject(file.Key, file.Value.Length))
            .ToList());
}
