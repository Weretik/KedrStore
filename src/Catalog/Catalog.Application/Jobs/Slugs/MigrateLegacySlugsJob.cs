using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Slugs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Jobs.Slugs;

/// <summary>
/// One-off move from legacy slugs (name + "-{id}") to stable KMU 2010 slugs. Entities are processed in ID order,
/// so collision suffixes are deterministic; each legacy slug goes to history. A rerun finds no legacy slugs and
/// changes nothing.
/// </summary>
public sealed class MigrateLegacySlugsJob(
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductCategory> categoryRepo,
    ISlugService slugService,
    ILogger<MigrateLegacySlugsJob> logger)
{
    private const int SaveBatchSize = 1_000;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var categories = await categoryRepo.ListAsync(new AllCategoriesIncludingDeletedSpec(), cancellationToken);
        await MigrateAsync(
            CatalogEntityType.Category,
            categories.Select(category => new Item(category.Id.Value, category.Name, category.Slug,
                (slug, now) => category.ChangeSlug(slug, now))).ToList(),
            cancellationToken);

        var products = await productRepo.ListAsync(new AllProductsIncludingDeletedSpec(), cancellationToken);
        await MigrateAsync(
            CatalogEntityType.Product,
            products.Select(product => new Item(product.Id.Value, product.Name, product.ProductSlug,
                (slug, now) => product.ChangeSlug(slug, now))).ToList(),
            cancellationToken);
    }

    private async Task MigrateAsync(CatalogEntityType entityType, IReadOnlyList<Item> items, CancellationToken cancellationToken)
    {
        var legacy = items.Where(item => IsLegacy(item.Slug, item.Id)).OrderBy(item => item.Id).ToList();
        logger.LogInformation("Slug migration for {EntityType}: {LegacyCount} of {Total} use a legacy slug.",
            entityType, legacy.Count, items.Count);

        if (legacy.Count == 0)
            return;

        // Legacy slugs are already taken: they become history and stay reserved for their entity.
        var allocator = await slugService.LoadAllocatorAsync(entityType, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var changed = 0;

        foreach (var item in legacy)
        {
            var newSlug = allocator.Allocate(item.Name);
            await slugService.RecordChangeAsync(entityType, item.Id, item.Slug, newSlug, now, cancellationToken);
            item.Apply(newSlug, now);

            if (++changed % SaveBatchSize == 0)
                await productRepo.SaveChangesAsync(cancellationToken);
        }

        await productRepo.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Slug migration for {EntityType} finished. Changed: {Changed}.", entityType, changed);
    }

    /// <summary>Legacy slugs end with "-{1C id}"; new slugs never carry the ID.</summary>
    public static bool IsLegacy(string slug, int id)
        => slug.EndsWith("-" + id.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

    private sealed record Item(int Id, string Name, string Slug, Action<string, DateTimeOffset> Apply);

    private sealed class AllCategoriesIncludingDeletedSpec : Specification<ProductCategory>
    {
        public AllCategoriesIncludingDeletedSpec() => Query.IgnoreQueryFilters();
    }

    private sealed class AllProductsIncludingDeletedSpec : Specification<Product>
    {
        public AllProductsIncludingDeletedSpec() => Query.IgnoreQueryFilters();
    }
}
