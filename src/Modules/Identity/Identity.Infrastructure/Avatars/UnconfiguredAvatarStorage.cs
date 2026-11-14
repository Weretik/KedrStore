namespace Identity.Infrastructure.Avatars;

/// <summary>Used when no CDN credentials are configured (local runs, tests); uploads answer 503.</summary>
public sealed class UnconfiguredAvatarStorage : IAvatarStorage
{
    public bool IsConfigured => false;

    public Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Avatar storage is not configured.");

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
