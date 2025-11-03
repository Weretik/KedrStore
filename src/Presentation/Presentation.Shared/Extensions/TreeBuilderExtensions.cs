using Application.Catalog.GetCategories;

namespace Presentation.Shared.Extensions;

public static class TreeBuilderExtensions
{
    public static List<TreeItemData<CategoryTreeDto>> ToTreeItems(this IEnumerable<CategoryTreeDto> source)
        => source.Select(Map).ToList() ?? [];

    private static TreeItemData<CategoryTreeDto> Map(CategoryTreeDto category)
        => new()
        {
            Text = category.Name,
            Value = category,
            Children = category.Children is { Count: > 0 }
                ? category.Children.ToTreeItems()
                : null
        };
}
