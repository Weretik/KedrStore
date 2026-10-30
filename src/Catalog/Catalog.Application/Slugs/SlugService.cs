using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Slugs;

public sealed class SlugService(IReadCatalogDbContext catalogDbContext) : ISlugService
{
    public async Task<SlugAllocator> LoadAllocatorAsync(CatalogEntityType entityType, CancellationToken cancellationToken)
    {
        var current = entityType == CatalogEntityType.Product
            ? await catalogDbContext.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(product => product.ProductSlug).ToListAsync(cancellationToken)
            : await catalogDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
                .Select(category => category.Slug).ToListAsync(cancellationToken);

        var history = await catalogDbContext.SlugHistory.AsNoTracking()
            .Where(entry => entry.EntityType == entityType)
            .Select(entry => entry.Slug)
            .ToListAsync(cancellationToken);

        return new SlugAllocator(entityType, current.Concat(history));
    }

    public async Task<int?> ResolveAsync(CatalogEntityType entityType, string slug, CancellationToken cancellationToken)
    {
        var normalized = slug.Trim().ToLowerInvariant();

        int? currentId = entityType == CatalogEntityType.Product
            ? await catalogDbContext.Products.AsNoTracking()
                .Where(product => product.ProductSlug == normalized)
                .Select(product => (int?)product.Id.Value).FirstOrDefaultAsync(cancellationToken)
            : await catalogDbContext.Categories.AsNoTracking()
                .Where(category => category.Slug == normalized)
                .Select(category => (int?)category.Id.Value).FirstOrDefaultAsync(cancellationToken);

        if (currentId is not null)
            return currentId;

        return await catalogDbContext.SlugHistory.AsNoTracking()
            .Where(entry => entry.EntityType == entityType && entry.Slug == normalized)
            .Select(entry => (int?)entry.EntityId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsTakenByOtherAsync(CatalogEntityType entityType, string slug, int entityId,
        CancellationToken cancellationToken)
    {
        var usedAsCurrent = entityType == CatalogEntityType.Product
            ? await catalogDbContext.Products.IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(product => product.ProductSlug == slug && product.Id != ProductId.From(entityId), cancellationToken)
            : await catalogDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(category => category.Slug == slug && category.Id != ProductCategoryId.From(entityId), cancellationToken);

        return usedAsCurrent || await catalogDbContext.SlugHistory.AsNoTracking()
            .AnyAsync(entry => entry.EntityType == entityType && entry.Slug == slug && entry.EntityId != entityId,
                cancellationToken);
    }

    public async Task RecordChangeAsync(CatalogEntityType entityType, int entityId, string oldSlug, string newSlug,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        // Tracked only: the caller saves together with the entity's own slug change in one SaveChanges.
        var history = catalogDbContext.SlugHistory;
        var entries = await history
            .Where(entry => entry.EntityType == entityType && (entry.Slug == newSlug || entry.Slug == oldSlug))
            .ToListAsync(cancellationToken);

        var reclaimed = entries.FirstOrDefault(entry => entry.Slug == newSlug);
        if (reclaimed is not null)
            history.Remove(reclaimed);

        var existing = entries.FirstOrDefault(entry => entry.Slug == oldSlug);
        if (existing is null)
            history.Add(SlugHistoryEntry.Create(entityType, oldSlug, entityId, now));
        else
            existing.Reassign(entityId, now);
    }
}
