namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed record CounterpartiesSyncResult(
    int Imported,
    int Updated,
    int Restored,
    int Deleted,
    int Skipped);
