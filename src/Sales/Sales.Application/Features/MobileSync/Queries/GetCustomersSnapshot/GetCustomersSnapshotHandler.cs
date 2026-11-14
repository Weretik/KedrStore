using BuildingBlocks.Application.Snapshots;
using Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;

namespace Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;

/// <summary>
/// Builds the customers snapshot: active counterparties ordered by ID with their category price rules (the first
/// rule of a category wins, as in the order workspace), so the same data always gives the same version.
/// </summary>
public sealed class GetCustomersSnapshotHandler(IReadSalesDbContext salesDbContext, TimeProvider timeProvider)
    : IQueryHandler<GetCustomersSnapshotQuery, Result<CustomersSnapshotDto>>
{
    public async ValueTask<Result<CustomersSnapshotDto>> Handle(GetCustomersSnapshotQuery query,
        CancellationToken cancellationToken)
    {
        var counterparties = await salesDbContext.Counterparties.AsNoTracking()
            .Where(counterparty => !counterparty.IsDeleted)
            .OrderBy(counterparty => counterparty.Id)
            .Select(counterparty => new
            {
                counterparty.Id,
                counterparty.Name,
                counterparty.Phone,
                counterparty.Email,
                counterparty.DefaultPriceTypeId
            })
            .ToListAsync(cancellationToken);

        var rules = (await salesDbContext.CounterpartyCategoryPriceTypes.AsNoTracking()
                .Select(rule => new { rule.CounterpartyId, rule.CategoryId, rule.PriceTypeId })
                .ToListAsync(cancellationToken))
            .ToLookup(rule => rule.CounterpartyId, StringComparer.Ordinal);

        var customers = counterparties
            .Select(counterparty => new CustomerSnapshotDto(
                counterparty.Id,
                counterparty.Name,
                counterparty.Phone,
                counterparty.Email,
                counterparty.DefaultPriceTypeId,
                rules[counterparty.Id]
                    .GroupBy(rule => rule.CategoryId)
                    .OrderBy(group => group.Key)
                    .Select(group => new CustomerSnapshotPriceRuleDto(group.Key, group.First().PriceTypeId))
                    .ToList(),
                Debt: null,
                Balance: null))
            .ToList();

        var version = SnapshotVersion.Compute(customers);
        return Result.Success(new CustomersSnapshotDto(version, timeProvider.GetUtcNow(), customers));
    }
}
