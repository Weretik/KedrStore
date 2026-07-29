using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Unidecode.NET;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCCategoryJob(IOneCClient oneC, ICatalogRepository<ProductCategory> categoryRepo,
    IOptionsSnapshot<RootCategoryId> options,
    IOptionsSnapshot<CategoryPresentationOptions> presentationOptions,
    ILogger<SyncOneCCategoryJob> logger)
{
    public async Task RunAsync(string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCCategoryJob started for {Root}", rootCategoryOneCId);

        if (string.Equals(rootCategoryOneCId, options.Value.CosmosRootCategoryId, StringComparison.Ordinal))
        {
            await EnsureCosmosCategoryAsync(cancellationToken);
            logger.LogInformation("SyncOneCCategoryJob finished for virtual Cosmos root.");
            return;
        }

        var hasNumericRootId = int.TryParse(rootCategoryOneCId.TrimStart('0'), out var rootCategoryId);
        var isHardwareRoot = string.Equals(
            rootCategoryOneCId,
            options.Value.HardwareRootCategoryId,
            StringComparison.Ordinal);
        var isDoorsRoot = string.Equals(
            rootCategoryOneCId,
            options.Value.DoorsRootCategoryId,
            StringComparison.Ordinal);

        // Select the manual grouping config based on the current root category branch.
        IReadOnlyList<ManualCategoryGroupOption> manualGroups = [];
        if (isHardwareRoot)
            manualGroups = options.Value.HardwareManualCategoryGroups;
        else if (isDoorsRoot)
            manualGroups = options.Value.DoorsManualCategoryGroups;

        var categoriesOneC = await oneC.GetCategoriesAsync(rootCategoryOneCId, cancellationToken);

        if (categoriesOneC.Count == 0 && manualGroups.Count == 0)
            return;

        var categories = CatalogMapper.MapCategory(
            categoriesOneC,
            hasNumericRootId ? rootCategoryId : null,
            isHardwareRoot,
            rootCategoryOneCId);
        if (manualGroups.Count > 0)
        {
            categories = ApplyManualHardwareHierarchy(
                categories,
                rootCategoryId,
                rootCategoryOneCId,
                manualGroups);
        }

        if (categoriesOneC.Count > 0)
        {
            await DeleteMissingAsync(categories, rootCategoryOneCId, cancellationToken);
        }
        else
        {
            // When 1C returns no categories, keep existing rows and upsert only root/manual groups.
            logger.LogWarning(
                "No categories from 1C for {Root}. DeleteMissing is skipped; only root/manual categories will be upserted.",
                rootCategoryOneCId);
        }

        await CreateOrUpsertCategoriesAsync(categories, rootCategoryOneCId, cancellationToken);

        logger.LogInformation("SyncOneCCategoryJob finished for {Root}", rootCategoryOneCId);
    }

    private async Task EnsureCosmosCategoryAsync(CancellationToken cancellationToken)
    {
        var cosmosCategoryId = ProductCategoryId.From(options.Value.CosmosCategoryId);
        var cosmosRootId = options.Value.CosmosRootCategoryId;
        var existing = await categoryRepo.GetByIdAsync(cosmosCategoryId, cancellationToken);
        var path = CategoryPath.From($"n{cosmosCategoryId.Value}");
        var presentation = ResolvePresentation(cosmosRootId, cosmosCategoryId.Value, "Космос", path);

        if (existing is null)
        {
            var category = ProductCategory.Create(
                cosmosCategoryId,
                cosmosRootId,
                "Космос",
                $"cosmos-{cosmosCategoryId.Value}",
                path);
            ApplyPresentation(category, presentation);
            await categoryRepo.AddAsync(category, cancellationToken);
        }
        else
        {
            if (!string.Equals(existing.ProductTypeIdOneC, cosmosRootId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"CosmosCategoryId {cosmosCategoryId.Value} is already used by root {existing.ProductTypeIdOneC}.");
            }

            existing.Update("Космос", $"cosmos-{cosmosCategoryId.Value}", path);
            ApplyPresentation(existing, presentation);
        }

        await categoryRepo.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteMissingAsync(IReadOnlyList<CategoryDto> categoryDtos, string rootCategoryOneCId,
        CancellationToken cancellationToken)
    {
        var importCategoryIds = categoryDtos
            .Select(c => ProductCategoryId.From(c.Id))
            .Distinct()
            .ToArray();

        var categorySpec  = new CategoriesByIdsSpec(importCategoryIds, rootCategoryOneCId,true);
        await categoryRepo.DeleteRangeAsync(categorySpec, cancellationToken);
    }

    private async Task CreateOrUpsertCategoriesAsync(IReadOnlyList<CategoryDto> categoryDtos, string rootCategoryOneCId,
        CancellationToken cancellationToken)
    {

        foreach (var item in categoryDtos)
        {
            var id = ProductCategoryId.From(item.Id);
            ProductCategoryId? parentId = item.ParentId == null ? null : ProductCategoryId.From(item.ParentId.Value);
            CategoryPath path = CategoryPath.From(item.Path);
            var existing = await categoryRepo.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var productCategory = ProductCategory.Create(
                    id: id,
                    productTypeIdOneC: rootCategoryOneCId,
                    name: item.Name,
                    slug: item.Slug,
                    path: path,
                    parentId: parentId
                );
                ApplyPresentation(productCategory, ResolvePresentation(rootCategoryOneCId, item.Id, item.Name, path));
                await categoryRepo.AddAsync(productCategory, cancellationToken);
            }
            else
            {
                existing.Update(
                    name: item.Name,
                    slug: item.Slug,
                    path: path,
                    parentId: parentId);
                ApplyPresentation(existing, ResolvePresentation(rootCategoryOneCId, item.Id, item.Name, path));
            }
        }
        await categoryRepo.SaveChangesAsync(cancellationToken);
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

    private static void ApplyPresentation(ProductCategory category, ResolvedCategoryPresentation presentation)
        => category.UpdatePresentationMetadata(
            presentation.ShortNameUk,
            presentation.ShortNameRu,
            presentation.SortOrder,
            presentation.Level);

    private static IReadOnlyList<CategoryDto> ApplyManualHardwareHierarchy(
        IReadOnlyList<CategoryDto> categories,
        int rootCategoryId,
        string rootCategoryOneCId,
        IReadOnlyList<ManualCategoryGroupOption> groups)
    {
        if (groups.Count == 0)
            return categories;

        var helper = new SlugHelper();

        var categoryMap = categories.ToDictionary(
            category => category.Id,
            category => new MutableCategory(category.Id, category.ProductTypeIdOneC, category.Name, category.Slug, category.ParentId, category.Path));

        foreach (var group in groups)
        {
            if (group.Id <= 0 || group.Id == rootCategoryId || string.IsNullOrWhiteSpace(group.Name))
                continue;

            var normalizedName = group.Name.Trim();
            var slug = GenerateSlug(normalizedName, group.Id, helper);

            if (!categoryMap.TryGetValue(group.Id, out var groupCategory))
            {
                groupCategory = new MutableCategory(
                    id: group.Id,
                    productTypeIdOneC: rootCategoryOneCId,
                    name: normalizedName,
                    slug: slug,
                    parentId: rootCategoryId,
                    path: string.Empty);

                categoryMap[group.Id] = groupCategory;
            }
            else
            {
                groupCategory.ProductTypeIdOneC = rootCategoryOneCId;
                groupCategory.Name = normalizedName;
                groupCategory.Slug = slug;
                groupCategory.ParentId = rootCategoryId;
            }

            foreach (var childId in group.ChildCategoryIds.Distinct())
            {
                if (childId <= 0 || childId == rootCategoryId || childId == group.Id)
                    continue;

                // Re-parent explicitly mapped categories under the configured manual parent.
                if (categoryMap.TryGetValue(childId, out var childCategory))
                    childCategory.ParentId = group.Id;
            }
        }

        var otherGroup = categoryMap.Values
            .FirstOrDefault(category =>
                string.Equals(category.Name, "Інше", StringComparison.OrdinalIgnoreCase));

        if (otherGroup is not null)
        {
            var manualGroupIds = groups
                .Where(group => group.Id > 0)
                .Select(group => group.Id)
                .ToHashSet();

            foreach (var category in categoryMap.Values)
            {
                if (category.Id == rootCategoryId || category.Id == otherGroup.Id || manualGroupIds.Contains(category.Id))
                    continue;

                // Any unmapped top-level category is moved under the fallback "Інше" group.
                if (category.ParentId is null || category.ParentId == rootCategoryId || !manualGroupIds.Contains(category.ParentId.Value))
                {
                    category.ParentId = otherGroup.Id;
                }
            }
        }

        var pathCache = new Dictionary<int, string>
        {
            [rootCategoryId] = $"n{rootCategoryId}"
        };

        // Rebuild hierarchical paths after manual re-parenting to keep tree invariants consistent.
        string BuildPath(int categoryId, HashSet<int> visiting)
        {
            if (pathCache.TryGetValue(categoryId, out var cachedPath))
                return cachedPath;

            if (!categoryMap.TryGetValue(categoryId, out var category))
                return $"n{rootCategoryId}.n{categoryId}";

            var parentId = category.ParentId;
            string path;

            if (parentId is null || parentId <= 0 || parentId == categoryId || !categoryMap.ContainsKey(parentId.Value))
            {
                path = $"n{rootCategoryId}.n{categoryId}";
            }
            else if (!visiting.Add(categoryId))
            {
                path = $"n{rootCategoryId}.n{categoryId}";
            }
            else
            {
                var parentPath = BuildPath(parentId.Value, visiting);
                visiting.Remove(categoryId);
                path = $"{parentPath}.n{categoryId}";
            }

            pathCache[categoryId] = path;
            return path;
        }

        foreach (var category in categoryMap.Values)
        {
            category.Path = category.Id == rootCategoryId
                ? $"n{rootCategoryId}"
                : BuildPath(category.Id, new HashSet<int>());
        }

        return categoryMap.Values
            .OrderBy(category => category.Path, StringComparer.Ordinal)
            .ThenBy(category => category.Id)
            .Select(category => new CategoryDto(
                category.Id,
                category.ProductTypeIdOneC,
                category.Name,
                category.Slug,
                category.ParentId,
                category.Path))
            .ToList();
    }

    private static string GenerateSlug(string name, int id, SlugHelper helper)
    {
        var normalized = helper.GenerateSlug(name.Unidecode()).Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = "category";

        return $"{normalized}-{id}";
    }

    private sealed class MutableCategory(
        int id,
        string productTypeIdOneC,
        string name,
        string slug,
        int? parentId,
        string path)
    {
        public int Id { get; } = id;
        public string ProductTypeIdOneC { get; set; } = productTypeIdOneC;
        public string Name { get; set; } = name;
        public string Slug { get; set; } = slug;
        public int? ParentId { get; set; } = parentId;
        public string Path { get; set; } = path;
    }
}
