namespace Identity.Infrastructure.Avatars;

/// <summary>Where processed avatar files live. Keys are relative to the avatars folder.</summary>
public interface IAvatarStorage
{
    bool IsConfigured { get; }
    Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
