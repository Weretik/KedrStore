using Catalog.Application.Features.RetailPrices.Shared;
using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Queries.GetRetailPriceStatus;

public sealed class GetRetailPriceStatusHandler(RetailPriceStatusReader status)
    : IQueryHandler<GetRetailPriceStatusQuery, Result<RetailPriceStatusDto>>
{
    public async ValueTask<Result<RetailPriceStatusDto>> Handle(GetRetailPriceStatusQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await status.ReadAsync(cancellationToken));
}
