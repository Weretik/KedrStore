using System.Collections.Concurrent;
using Identity.Infrastructure.Avatars;

namespace IntegrationTests.Identity.Avatars;

/// <summary>Stands in for the CDN in API tests.</summary>
internal sealed class InMemoryAvatarStorage : IAvatarStorage
{
    public ConcurrentDictionary<string, byte[]> Files { get; } = new();

    public bool IsConfigured => true;

    public Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default)
    {
        Files[key] = content;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        Files.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
