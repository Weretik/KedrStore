using Sales.Infrastructure.Integrations.OneC;
using Sales.Infrastructure.Integrations.OneC.Counterparties;

namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed record SalesCustomersFullSyncResult(
    CounterpartiesSyncResult Counterparties,
    CounterpartyCategoryPriceTypesSyncResult PriceRules);
