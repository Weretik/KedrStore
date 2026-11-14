namespace Sales.Infrastructure.Integrations.OneC.Counterparties;

public sealed record CounterpartiesSyncResult(
    int Imported,
    int Updated,
    int Restored,
    int Deleted,
    int Skipped,
    int Unchanged = 0);
