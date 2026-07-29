using Sales.Infrastructure.Integrations.OneC.Services;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed record SalesCustomersFullSyncResult(
    CounterpartiesSyncResult Counterparties,
    CounterpartyCategoryPriceTypesSyncResult PriceRules);
