namespace Catalog.Application.Integrations.OneC.Options;

public sealed class RootCategoryId
{
    public required string DoorsRootCategoryId { get; init; }
    public required string HardwareRootCategoryId { get; init; }
    public required string CosmosRootCategoryId { get; init; }
    public required int CosmosCategoryId { get; init; }
    public List<ManualCategoryGroupOption> HardwareManualCategoryGroups { get; init; } = [];
    public List<ManualCategoryGroupOption> DoorsManualCategoryGroups { get; init; } = [];
}
