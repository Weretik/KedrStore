using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Slugs;

public interface ISlugService
{
    /// <summary>Loads every current and former slug of the type, for allocating slugs of new entities.</summary>
    Task<SlugAllocator> LoadAllocatorAsync(CatalogEntityType entityType, CancellationToken cancellationToken);

    /// <summary>Returns the entity ID for a current slug (active entities only) or a former slug.</summary>
    Task<int?> ResolveAsync(CatalogEntityType entityType, string slug, CancellationToken cancellationToken);

    /// <summary>Whether the slug is a current or former slug of an entity other than <paramref name="entityId"/>.</summary>
    Task<bool> IsTakenByOtherAsync(CatalogEntityType entityType, string slug, int entityId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records <paramref name="oldSlug"/> as a former slug of the entity and drops <paramref name="newSlug"/> from
    /// its own history if the entity takes an old slug back. Changes are saved by the caller's unit of work.
    /// </summary>
    Task RecordChangeAsync(CatalogEntityType entityType, int entityId, string oldSlug, string newSlug,
        DateTimeOffset now, CancellationToken cancellationToken);
}
