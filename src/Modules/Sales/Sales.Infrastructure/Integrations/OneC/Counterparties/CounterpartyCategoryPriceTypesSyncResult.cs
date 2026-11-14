namespace Sales.Infrastructure.Integrations.OneC.Counterparties;

public sealed record CounterpartyCategoryPriceTypesSyncResult(
    int ImportedOrUpdated,
    int Deleted,
    int Skipped);
