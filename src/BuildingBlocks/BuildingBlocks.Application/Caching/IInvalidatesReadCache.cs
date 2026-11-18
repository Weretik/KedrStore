namespace BuildingBlocks.Application.Caching;

/// <summary>
/// A command that changes data some cached read shows. After the command succeeds,
/// <c>ReadCacheInvalidationBehavior</c> drops every answer carrying one of <see cref="InvalidatedCacheTags"/>.
/// </summary>
public interface IInvalidatesReadCache
{
    IReadOnlyCollection<string> InvalidatedCacheTags { get; }
}
