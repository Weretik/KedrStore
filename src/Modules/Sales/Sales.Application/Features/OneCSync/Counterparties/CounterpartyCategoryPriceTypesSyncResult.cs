namespace Sales.Application.Features.OneCSync.Counterparties;

public sealed record CounterpartyCategoryPriceTypesSyncResult(
    int ImportedOrUpdated,
    int Deleted,
    int Skipped);
