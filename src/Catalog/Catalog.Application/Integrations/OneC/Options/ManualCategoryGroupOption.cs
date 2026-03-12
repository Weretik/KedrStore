namespace Catalog.Application.Integrations.OneC.Options;

public sealed class ManualCategoryGroupOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public List<int> ChildCategoryIds { get; init; } = [];
}
