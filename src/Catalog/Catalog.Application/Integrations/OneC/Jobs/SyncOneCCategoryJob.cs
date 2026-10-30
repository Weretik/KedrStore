using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Application.Integrations.OneC.Sync;
using Catalog.Application.Slugs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

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
        await ApplyAsync(categories, rootCategoryOneCId, allowDeletion, counters, cancellationToken);
        counters.Log(logger, Phase, rootCategoryOneCId);
    }

    private async Task EnsureCosmosCategoryAsync(CancellationToken cancellationToken)
    {
        var cosmosCategoryId = ProductCategoryId.From(options.Value.CosmosCategoryId);
        var cosmosRootId = options.Value.CosmosRootCategoryId;
        var existing = (await categoryRepo.ListAsync(new CategoriesByRootSpec(cosmosRootId), cancellationToken))
            .FirstOrDefault(category => category.Id == cosmosCategoryId);
        var path = CategoryPath.From($"n{cosmosCategoryId.Value}");
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
            var category = ProductCategory.Create(cosmosCategoryId, cosmosRootId, CosmosName, slugs.Allocate(CosmosName), path);
            ApplyPresentation(category, presentation);
            await categoryRepo.AddAsync(category, cancellationToken);
            return;
        }

        existing.Restore(now);
        existing.UpdateFromOneC(CosmosName, path, null, now);
        ApplyPresentation(existing, presentation);
        await categoryRepo.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Creates, updates, restores, and soft-deletes the categories of the root in one save.</summary>
    private async Task ApplyAsync(IReadOnlyList<CategoryDto> categoryDtos, string rootCategoryOneCId,
        bool allowDeletion, ImportCounters counters, CancellationToken cancellationToken)
    {
        var existingById = (await categoryRepo.ListAsync(new CategoriesByRootSpec(rootCategoryOneCId), cancellationToken))
            .ToDictionary(category => category.Id.Value);
        var now = DateTimeOffset.UtcNow;
        var created = new List<ProductCategory>();
        SlugAllocator? slugs = null;

        foreach (var item in categoryDtos)
        {
            ProductCategoryId? parentId = item.ParentId == null ? null : ProductCategoryId.From(item.ParentId.Value);
            var path = CategoryPath.From(item.Path);
            var presentation = ResolvePresentation(rootCategoryOneCId, item.Id, item.Name, path);

            if (existingById.TryGetValue(item.Id, out var existing))
            {
                var restored = existing.Restore(now);
                var updated = existing.UpdateFromOneC(item.Name, path, parentId, now);
                updated |= ApplyPresentation(existing, presentation);

                if (restored) counters.Restored++;
                else if (updated) counters.Updated++;
                else counters.Unchanged++;
                continue;
            }

            slugs ??= await slugService.LoadAllocatorAsync(CatalogEntityType.Category, cancellationToken);
            var category = ProductCategory.Create(
                id: ProductCategoryId.From(item.Id),
                productTypeIdOneC: rootCategoryOneCId,
                name: item.Name,
                slug: slugs.Allocate(item.Name),
                path: path,
                parentId: parentId);
            ApplyPresentation(category, presentation);
            created.Add(category);
            counters.Created++;
        }

        if (allowDeletion)
            MarkMissingDeleted(existingById.Values, categoryDtos, rootCategoryOneCId, counters, now);

        if (created.Count > 0)
            await categoryRepo.AddRangeAsync(created, cancellationToken);

        await categoryRepo.SaveChangesAsync(cancellationToken);
    }

    private void MarkMissingDeleted(IEnumerable<ProductCategory> existing, IReadOnlyList<CategoryDto> categoryDtos,
        string rootCategoryOneCId, ImportCounters counters, DateTimeOffset now)
    {
        // Root and manual groups are part of categoryDtos, so they are never missing.
        var receivedIds = categoryDtos.Select(category => category.Id).ToHashSet();
        var active = existing.Where(category => !category.IsDeleted).ToList();
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

    private static bool ApplyPresentation(ProductCategory category, ResolvedCategoryPresentation presentation)
        => category.UpdatePresentationMetadata(
            presentation.ShortNameUk,
            presentation.ShortNameRu,
            presentation.SortOrder,
            presentation.Level);
}
