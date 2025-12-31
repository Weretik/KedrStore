using Catalog.Application.Features.Category.Queries.GetCategories;

namespace Presentation.Shared.States.Category;

[FeatureState]
public sealed record CategoryState(
    CategoryFilter Filter,
    bool IsLoading = false,
    string? Error = null,
    IReadOnlyList<CategoryTreeDto>? QueryResult = null)
{
    public CategoryState() : this(new CategoryFilter(), false, null, null) { }

}
