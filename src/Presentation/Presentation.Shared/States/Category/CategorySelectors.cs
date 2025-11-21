using Application.Catalog.GetCategories;
using Presentation.Shared.Extensions;
using Presentation.Shared.States.Catalog;

namespace Presentation.Shared.States.Category;

public static class CategorySelectors
{
    public static bool IsLoading(this CategoryState state) => state.IsLoading;
    public static string? GetError(this CategoryState state) => state.Error;
    public static bool HasError(this CategoryState state) => state.Error is not null;

    public static IReadOnlyCollection<TreeItemData<CategoryTreeDto>> GetCategories(this CategoryState state)
    {
        var categoryList = state.QueryResult;
        if (categoryList is null) return [];

        return categoryList.ToTreeItems();
    }
}
