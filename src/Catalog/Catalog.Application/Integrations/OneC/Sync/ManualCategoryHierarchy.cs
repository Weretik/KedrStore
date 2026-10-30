using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Options;

namespace Catalog.Application.Integrations.OneC.Sync;

/// <summary>Re-parents 1C categories under the configured manual groups and rebuilds their paths.</summary>
public static class ManualCategoryHierarchy
{
    private const string OtherGroupName = "Інше";

    public static IReadOnlyList<CategoryDto> Apply(
        IReadOnlyList<CategoryDto> categories,
        int rootCategoryId,
        string rootCategoryOneCId,
        IReadOnlyList<ManualCategoryGroupOption> groups)
    {
        if (groups.Count == 0)
            return categories;

        var categoryMap = categories.ToDictionary(
            category => category.Id,
            category => new MutableCategory(category.Id, category.ProductTypeIdOneC, category.Name, category.ParentId, category.Path));

        foreach (var group in groups)
        {
            if (group.Id <= 0 || group.Id == rootCategoryId || string.IsNullOrWhiteSpace(group.Name))
                continue;

            var normalizedName = group.Name.Trim();

            if (!categoryMap.TryGetValue(group.Id, out var groupCategory))
            {
                groupCategory = new MutableCategory(group.Id, rootCategoryOneCId, normalizedName, rootCategoryId, string.Empty);
                categoryMap[group.Id] = groupCategory;
            }
            else
            {
                groupCategory.ProductTypeIdOneC = rootCategoryOneCId;
                groupCategory.Name = normalizedName;
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

        MoveUnmappedUnderOther(categoryMap, groups, rootCategoryId);
        RebuildPaths(categoryMap, rootCategoryId);

        return categoryMap.Values
            .OrderBy(category => category.Path, StringComparer.Ordinal)
            .ThenBy(category => category.Id)
            .Select(category => new CategoryDto(
                category.Id,
                category.ProductTypeIdOneC,
                category.Name,
                category.ParentId,
                category.Path))
            .ToList();
    }

    private static void MoveUnmappedUnderOther(
        Dictionary<int, MutableCategory> categoryMap,
        IReadOnlyList<ManualCategoryGroupOption> groups,
        int rootCategoryId)
    {
        var otherGroup = categoryMap.Values
            .FirstOrDefault(category => string.Equals(category.Name, OtherGroupName, StringComparison.OrdinalIgnoreCase));

        if (otherGroup is null)
            return;

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
                category.ParentId = otherGroup.Id;
        }
    }

    private static void RebuildPaths(Dictionary<int, MutableCategory> categoryMap, int rootCategoryId)
    {
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
                : BuildPath(category.Id, []);
        }
    }

    private sealed class MutableCategory(int id, string productTypeIdOneC, string name, int? parentId, string path)
    {
        public int Id { get; } = id;
        public string ProductTypeIdOneC { get; set; } = productTypeIdOneC;
        public string Name { get; set; } = name;
        public int? ParentId { get; set; } = parentId;
        public string Path { get; set; } = path;
    }
}
