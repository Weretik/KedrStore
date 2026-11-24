using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;
using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

namespace Sales.Application.Abstractions.QuickOrders;

/// <summary>Admin reads of site orders: projections, never the aggregate (CQRS read side).</summary>
public interface IQuickOrderReadService
{
    Task<PagedResult<List<QuickOrderListItemDto>>> GetListAsync(QuickOrderListRequest request, CancellationToken cancellationToken);

    Task<QuickOrderDetailDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
}
