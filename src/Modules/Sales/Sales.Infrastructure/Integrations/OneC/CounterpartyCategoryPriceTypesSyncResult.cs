namespace Sales.Infrastructure.Integrations.OneC;

public sealed record CounterpartyCategoryPriceTypesSyncResult(
    int ImportedOrUpdated,
    int Deleted,
    int Skipped);
