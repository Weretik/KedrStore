using Sales.Application.Abstractions.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

public sealed class GetQuickOrderListHandler(IQuickOrderReadService reader)
    : IQueryHandler<GetQuickOrderListQuery, Result<PagedResult<List<QuickOrderListItemDto>>>>
{
    public async ValueTask<Result<PagedResult<List<QuickOrderListItemDto>>>> Handle(
        GetQuickOrderListQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await reader.GetListAsync(query.Request, cancellationToken));
}
