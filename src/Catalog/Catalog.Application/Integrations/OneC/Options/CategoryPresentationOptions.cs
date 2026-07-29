namespace Catalog.Application.Integrations.OneC.Options;

public sealed class CategoryPresentationOptions
{
    public List<CategoryPresentationOption> Categories { get; init; } = [];
}

public sealed class CategoryPresentationOption
{
    public required string ProductTypeIdOneC { get; init; }
    public required int CategoryId { get; init; }
    public required string ShortNameUk { get; init; }
    public required string ShortNameRu { get; init; }
    public required int SortOrder { get; init; }
}
