using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Application.Features.OneCSync.Source;
using Catalog.Application.Features.OneCSync.Mapping;
using Catalog.Application.Features.OneCSync.Sync;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Application.Features.OneCSync.Sync.Categories;

namespace Catalog.Application.Features.OneCSync.Jobs;

public sealed class SyncOneCCategoryJob(IOneCClient oneC, ICatalogRepository<ProductCategory> categoryRepo,
    ISlugService slugService,
    IOptionsSnapshot<RootCategoryId> options,
    IOptionsSnapshot<CategoryPresentationOptions> presentationOptions,
    ILogger<SyncOneCCategoryJob> logger)
{
    private const string Phase = "Category import";
    private const string CosmosName = "Космос";

    public async Task RunAsync(string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        if (string.Equals(rootCategoryOneCId, options.Value.CosmosRootCategoryId, StringComparison.Ordinal))
        {
            await EnsureCosmosCategoryAsync(cancellationToken);
            return;
        }

        var hasNumericRootId = int.TryParse(rootCategoryOneCId.TrimStart('0'), out var rootCategoryId);
        var isHardwareRoot = string.Equals(rootCategoryOneCId, options.Value.HardwareRootCategoryId, StringComparison.Ordinal);
        var isDoorsRoot = string.Equals(rootCategoryOneCId, options.Value.DoorsRootCategoryId, StringComparison.Ordinal);

        // Select the manual grouping config based on the current root category branch.
        IReadOnlyList<ManualCategoryGroupOption> manualGroups = [];
        if (isHardwareRoot)
            manualGroups = options.Value.HardwareManualCategoryGroups;
        else if (isDoorsRoot)
            manualGroups = options.Value.DoorsManualCategoryGroups;

        var fetchTimer = Stopwatch.StartNew();
        var categoriesOneC = await oneC.GetCategoriesAsync(rootCategoryOneCId, cancellationToken);
        logger.LogInformation(
            "Received {Count} categories from 1C for {Root} in {ElapsedMs} ms.",
            categoriesOneC.Count, rootCategoryOneCId, fetchTimer.ElapsedMilliseconds);

        if (categoriesOneC.Count == 0 && manualGroups.Count == 0)
            return;

        var categories = CatalogMapper.MapCategory(
            categoriesOneC,
            hasNumericRootId ? rootCategoryId : null,
            isHardwareRoot,
            rootCategoryOneCId);
        if (manualGroups.Count > 0)
            categories = ManualCategoryHierarchy.Apply(categories, rootCategoryId, rootCategoryOneCId, manualGroups);

        // When 1C returns no categories, keep existing rows and upsert only root/manual groups.
        var allowDeletion = categoriesOneC.Count > 0;
        if (!allowDeletion)
        {
            logger.LogWarning(
                "No categories from 1C for {Root}. Soft deletion is skipped; only root/manual categories will be upserted.",
                rootCategoryOneCId);
        }

        var counters = new ImportCounters { Received = categories.Count };
        await ApplyAsync(categories, rootCategoryOneCId, isHardwareRoot, allowDeletion, counters, cancellationToken);
        counters.Log(logger, Phase, rootCategoryOneCId);
    }

    private async Task EnsureCosmosCategoryAsync(CancellationToken cancellationToken)
    {
        var cosmosCategoryId = ProductCategoryId.Create(options.Value.CosmosCategoryId);
        var cosmosRootId = options.Value.CosmosRootCategoryId;
        var existing = (await categoryRepo.ListAsync(new CategoriesByRootSpec(cosmosRootId), cancellationToken))
            .FirstOrDefault(category => category.Id == cosmosCategoryId);
        var path = CategoryPath.Create($"n{cosmosCategoryId.Value}");
        var presentation = ResolvePresentation(cosmosRootId, cosmosCategoryId.Value, CosmosName, path);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            if (await categoryRepo.GetByIdAsync(cosmosCategoryId, cancellationToken) is { } owner)
            {
                throw new InvalidOperationException(
                    $"CosmosCategoryId {cosmosCategoryId.Value} is already used by root {owner.ProductTypeIdOneC}.");
            }

            var slugs = await slugService.LoadAllocatorAsync(CatalogEntityType.Category, cancellationToken);
            // The Cosmos category is built from fixed values, so its rules always pass.
            var category = ProductCategory.Create(cosmosCategoryId, cosmosRootId, CosmosName, slugs.Allocate(CosmosName),
                path).Value;
            ApplyPresentation(category, presentation).ThrowIfFailure();
            await categoryRepo.AddAsync(category, cancellationToken);
            await categoryRepo.SaveChangesAsync(cancellationToken);
            return;
        }

        existing.Restore(now);
        existing.UpdateFromOneC(CosmosName, path, null, now).ThrowIfFailure();
        ApplyPresentation(existing, presentation).ThrowIfFailure();
        await categoryRepo.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Creates, updates, restores, and soft-deletes the categories of the root in one save.</summary>
    private async Task ApplyAsync(IReadOnlyList<CategoryDto> categoryDtos, string rootCategoryOneCId,
        bool isHardwareRoot, bool allowDeletion, ImportCounters counters, CancellationToken cancellationToken)
    {
        var existingById = (await categoryRepo.ListAsync(new CategoriesByRootSpec(rootCategoryOneCId), cancellationToken))
            .ToDictionary(category => category.Id.Value);
        var now = DateTimeOffset.UtcNow;
        var created = new List<ProductCategory>();
        SlugAllocator? slugs = null;

        foreach (var item in categoryDtos)
        {
            ProductCategoryId? parentId = item.ParentId == null ? null : ProductCategoryId.Create(item.ParentId.Value);
            var path = CategoryPath.Create(item.Path);
            if (existingById.TryGetValue(item.Id, out var existing))
            {
                var fromOneC = existing.UpdateFromOneC(item.Name, path, parentId, now);
                // The level comes from where the category sits now: a place set by hand wins over 1C (spec 011 MC-09).
                var shown = fromOneC.IsSuccess
                    ? ApplyPresentation(existing, ResolvePresentation(rootCategoryOneCId, item.Id, item.Name, existing.Path))
                    : fromOneC;
                if (shown.IsFailure)
                {
                    logger.LogWarning("Skipping category {CategoryId} from 1C: {Error}", item.Id, shown.Error!.Message);
                    counters.Unchanged++;
                    continue;
                }

                var restored = existing.Restore(now);
                var updated = fromOneC.Value | shown.Value;

                if (restored) counters.Restored++;
                else if (updated) counters.Updated++;
                else counters.Unchanged++;
                continue;
            }

            slugs ??= await slugService.LoadAllocatorAsync(CatalogEntityType.Category, cancellationToken);
            var createdCategory = ProductCategory.Create(
                id: ProductCategoryId.Create(item.Id),
                productTypeIdOneC: rootCategoryOneCId,
                name: item.Name,
                slug: slugs.Allocate(item.Name),
                path: path,
                parentId: parentId);
            var presented = createdCategory.IsSuccess
                ? ApplyPresentation(createdCategory.Value, ResolvePresentation(rootCategoryOneCId, item.Id, item.Name, path))
                : createdCategory.Error!;
            if (presented.IsFailure)
            {
                logger.LogWarning("Skipping new category {CategoryId} from 1C: {Error}", item.Id, presented.Error!.Message);
                continue;
            }

            var category = createdCategory.Value;
            created.Add(category);
            counters.Created++;
        }

        InheritSiteVisibility(created, existingById, isHardwareRoot, now);

        if (allowDeletion)
            MarkMissingDeleted(existingById.Values, categoryDtos, rootCategoryOneCId, counters, now);

        // Paths follow the parents: a category moved by hand, a new 1C category under it, and a manual category under
        // a 1C parent that 1C moved all sit where their parent is now (spec catalog/011 MC-05, MC-09).
        var tree = await categoryRepo.ListAsync(new WholeCategoryTreeSpec(), cancellationToken);
        CategoryTreePlacement.Rebuild([.. tree, .. created], now);

        if (created.Count > 0)
            await categoryRepo.AddRangeAsync(created, cancellationToken);

        await categoryRepo.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// A new category takes its parent's «show on site» value (a new top category is shown only in the hardware root,
    /// spec catalog/007 R-18) and its parent's «Закрити від пошуку» (spec catalog/010 R-01). Parents come first by path,
    /// so a new child of a new parent sees the parent's values.
    /// </summary>
    private static void InheritSiteVisibility(IReadOnlyList<ProductCategory> created,
        IReadOnlyDictionary<int, ProductCategory> existingById, bool isHardwareRoot, DateTimeOffset now)
    {
        var parents = existingById.Values.ToDictionary(category => category.Id);
        foreach (var category in created.OrderBy(category => category.Path.Value.Length))
        {
            var parent = category.ParentId is { } parentId ? parents.GetValueOrDefault(parentId) : null;
            category.SetShownOnSite(parent?.IsShownOnSite ?? isHardwareRoot, now);
            category.SetClosedFromSearch(parent?.IsClosedFromSearch ?? false, now);
            parents[category.Id] = category;
        }
    }

    private void MarkMissingDeleted(IEnumerable<ProductCategory> existing, IReadOnlyList<CategoryDto> categoryDtos,
        string rootCategoryOneCId, ImportCounters counters, DateTimeOffset now)
    {
        // Root and manual groups are part of categoryDtos, so they are never missing.
        var receivedIds = categoryDtos.Select(category => category.Id).ToHashSet();
        // A category made by hand is never in 1C and never deleted by the sync (spec catalog/011 MC-05).
        var active = existing.Where(category => !category.IsDeleted && !category.IsManual).ToList();
        var missing = active.Where(category => !receivedIds.Contains(category.Id.Value)).ToList();

        if (!MassDeletionGuard.Allows(active.Count, missing.Count))
        {
            counters.GuardBlocked = missing.Count;
            logger.LogError(
                "Mass deletion guard blocked soft-deleting {MissingCount} of {ActiveCount} active categories for {Root}. Nothing was deleted.",
                missing.Count, active.Count, rootCategoryOneCId);
            return;
        }

        foreach (var category in missing)
        {
            if (category.MarkDeleted(now))
                counters.Deleted++;
        }
    }

    private ResolvedCategoryPresentation ResolvePresentation(
        string rootCategoryOneCId,
        int categoryId,
        string sourceName,
        CategoryPath path)
    {
        var level = path.Value.Count(character => character == '.');
        return new CategoryPresentationResolver(presentationOptions.Value)
            .Resolve(rootCategoryOneCId, categoryId, sourceName, level);
    }

    private static DomainResult<bool> ApplyPresentation(ProductCategory category, ResolvedCategoryPresentation presentation)
        => category.UpdatePresentationMetadata(
            presentation.ShortNameUk,
            presentation.ShortNameRu,
            presentation.SortOrder,
            presentation.Level);

    /// <summary>Every category, deleted ones included, tracked, for rebuilding the paths.</summary>
    private sealed class WholeCategoryTreeSpec : Specification<ProductCategory>
    {
        public WholeCategoryTreeSpec() => Query.IgnoreQueryFilters();
    }
}
