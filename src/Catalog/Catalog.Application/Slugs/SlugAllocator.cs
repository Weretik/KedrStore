using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Slugs;

/// <summary>
/// Hands out unique slugs for one entity type during a run. The taken set holds current slugs and history,
/// so a former slug is never given to another entity.
/// </summary>
public sealed class SlugAllocator(CatalogEntityType entityType, IEnumerable<string> takenSlugs)
{
    private readonly HashSet<string> _taken = new(takenSlugs, StringComparer.Ordinal);

    public CatalogEntityType EntityType { get; } = entityType;

    public bool IsTaken(string slug) => _taken.Contains(slug);

    public void Take(string slug) => _taken.Add(slug);

    public string Allocate(string? name)
    {
        var baseSlug = SlugText.FromName(name, EntityType);
        for (var number = 1; ; number++)
        {
            var candidate = SlugText.WithSuffix(baseSlug, number);
            if (_taken.Add(candidate))
                return candidate;
        }
    }
}
