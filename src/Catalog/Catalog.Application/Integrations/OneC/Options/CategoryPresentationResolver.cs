namespace Catalog.Application.Integrations.OneC.Options;

public sealed class CategoryPresentationResolver
{
    private readonly IReadOnlyDictionary<(string ProductTypeIdOneC, int CategoryId), CategoryPresentationOption> _configured;

    public CategoryPresentationResolver(CategoryPresentationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configured = options.Categories
            .GroupBy(item => (item.ProductTypeIdOneC, item.CategoryId))
            .ToDictionary(
                group => group.Key,
                group => group.Count() == 1
                    ? group.Single()
                    : throw new InvalidOperationException(
                        $"Duplicate category presentation metadata for root '{group.Key.ProductTypeIdOneC}' and category '{group.Key.CategoryId}'."));
    }

    public ResolvedCategoryPresentation Resolve(
        string productTypeIdOneC,
        int categoryId,
        string sourceName,
        int level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productTypeIdOneC);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        if (_configured.TryGetValue((productTypeIdOneC, categoryId), out var configured))
        {
            return new ResolvedCategoryPresentation(
                configured.ShortNameUk,
                configured.ShortNameRu,
                configured.SortOrder,
                level);
        }

        return new ResolvedCategoryPresentation(sourceName, sourceName, int.MaxValue, level);
    }
}

public sealed record ResolvedCategoryPresentation(
    string ShortNameUk,
    string ShortNameRu,
    int SortOrder,
    int Level);
