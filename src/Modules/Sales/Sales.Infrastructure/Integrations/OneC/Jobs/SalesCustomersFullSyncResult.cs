using Sales.Infrastructure.Integrations.OneC;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed record SalesCustomersFullSyncResult(
    CounterpartiesSyncResult Counterparties,
    CounterpartyCategoryPriceTypesSyncResult PriceRules);
