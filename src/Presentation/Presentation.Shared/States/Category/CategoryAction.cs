using Application.Catalog.GetCategories;

namespace Presentation.Shared.States.Category;

public static class CategoryAction
{
    public sealed record Load;
    public sealed record LoadSuccess(IReadOnlyList<CategoryTreeDto> QueryResult);
    public sealed record LoadFailure(string Error);
    public sealed record SetFilter(CategoryFilter Filter);
    public sealed record SetProductTypeId(int? ProductTypeId);

}
