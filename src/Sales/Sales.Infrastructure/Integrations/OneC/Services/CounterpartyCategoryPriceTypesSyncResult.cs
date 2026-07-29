namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed record CounterpartyCategoryPriceTypesSyncResult(
    int ImportedOrUpdated,
    int Deleted,
    int Skipped);
