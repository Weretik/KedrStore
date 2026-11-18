using Sales.Application.Features.OneCSync.Counterparties;

namespace Sales.Application.Features.OneCSync.Jobs;

public sealed record SalesCustomersFullSyncResult(
    CounterpartiesSyncResult Counterparties,
    CounterpartyCategoryPriceTypesSyncResult PriceRules);
