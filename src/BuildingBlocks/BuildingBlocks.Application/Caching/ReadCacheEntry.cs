namespace BuildingBlocks.Application.Caching;

/// <summary>
/// What a cached read is called and how long it lives. <see cref="Name"/> is a low-cardinality label for traces
/// (<c>catalog.category-tree</c>); <see cref="Key"/> adds the read's arguments; <see cref="Tag"/> is what a command drops.
/// </summary>
public readonly record struct ReadCacheEntry
{
    private ReadCacheEntry(string name, string key, string tag, TimeSpan lifetime)
    {
        Name = name;
        Key = key;
        Tag = tag;
        Lifetime = lifetime;
    }

    public string Name { get; }
    public string Key { get; }
    public string Tag { get; }
    public TimeSpan Lifetime { get; }

    /// <param name="name">Stable name of the read, also the key prefix.</param>
    /// <param name="tag">Tag that the commands changing this data invalidate.</param>
    /// <param name="lifetime">How long an answer lives; other instances see a change only after it (ADR-0016).</param>
    /// <param name="variant">The read's arguments that change the answer (language, id), or null.</param>
    public static ReadCacheEntry Create(string name, string tag, TimeSpan lifetime, string? variant = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);
        return new ReadCacheEntry(name, variant is null ? name : $"{name}:{variant}", tag, lifetime);
    }
}
