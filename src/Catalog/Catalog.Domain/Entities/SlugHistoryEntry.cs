using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

/// <summary>A former slug of a product or category. It keeps resolving to its entity and is never reused.</summary>
public class SlugHistoryEntry : IAggregateRoot
{
    public CatalogEntityType EntityType { get; private set; }
    public string Slug { get; private set; } = null!;
    public int EntityId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }

    private SlugHistoryEntry() { }

    public static SlugHistoryEntry Create(CatalogEntityType entityType, string slug, int entityId, DateTimeOffset changedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entityId);

        return new SlugHistoryEntry
        {
            EntityType = entityType,
            Slug = slug.Trim(),
            EntityId = entityId,
            ChangedAt = changedAt
        };
    }

    /// <summary>Points a former slug at a new owner after the owner took it back (Admin set an old slug again).</summary>
    public void Reassign(int entityId, DateTimeOffset changedAt)
    {
        EntityId = entityId;
        ChangedAt = changedAt;
    }
}
