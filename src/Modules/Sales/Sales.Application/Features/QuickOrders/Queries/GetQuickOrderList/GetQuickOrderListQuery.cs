namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

public sealed record GetQuickOrderListQuery(QuickOrderListRequest Request)
    : IQuery<Result<PagedResult<List<QuickOrderListItemDto>>>>;
