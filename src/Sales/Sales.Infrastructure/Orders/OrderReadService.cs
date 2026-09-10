using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetById.DTOs;
using Sales.Application.Features.Orders.GetList.DTOs;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderReadService(SalesDbContext dbContext) : IOrderReadService
{
    public async Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
        OrderListRequest request,
        CancellationToken cancellationToken)
    {
        var query =
            from order in dbContext.Orders.AsNoTracking()
            join counterparty in dbContext.Counterparties.IgnoreQueryFilters().AsNoTracking()
                on order.CounterpartyId equals counterparty.Id
            join sync in dbContext.OneCOrderSyncs.AsNoTracking()
                on order.Id equals sync.OrderId
            where request.CounterpartyId == null || order.CounterpartyId == request.CounterpartyId
            orderby order.CreatedAt descending, order.Id descending
            select new OrderListItemDto(
                order.Id.Value,
                order.OrderNumber,
                order.CounterpartyId,
                counterparty.Name,
                order.CreatedAt,
                order.Lines.Count,
                order.Lines.Sum(line => line.Amount),
                sync.Status,
                sync.OneCDocumentNumber);

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var totalPages = totalRecords == 0
            ? 0
            : (long)Math.Ceiling(totalRecords / (double)request.PageSize);
        var page = new PagedResult<List<OrderListItemDto>>(
            new PagedInfo(request.Page, request.PageSize, totalPages, totalRecords),
            rows);

        return Result.Success(page);
    }

    public async Task<Result<OrderDetailDto>> GetByIdAsync(
        long orderId,
        CancellationToken cancellationToken)
    {
        var storageId = OrderId.FromStorage(orderId);
        var detail = await (
            from order in dbContext.Orders.AsNoTracking()
            join counterparty in dbContext.Counterparties.IgnoreQueryFilters().AsNoTracking()
                on order.CounterpartyId equals counterparty.Id
            join sync in dbContext.OneCOrderSyncs.AsNoTracking()
                on order.Id equals sync.OrderId
            where order.Id == storageId
            select new OrderDetailDto(
                order.Id.Value,
                order.OrderNumber,
                order.CreatedAt,
                new OrderCounterpartyDto(counterparty.Id, counterparty.Name, counterparty.Phone),
                order.Comment,
                order.Lines
                    .OrderBy(line => line.Id)
                    .Select(line => new OrderLineDto(
                        line.ProductId,
                        line.ProductName,
                        line.Quantity,
                        line.Amount))
                    .ToList(),
                order.Lines.Sum(line => line.Amount),
                new OrderSyncDto(sync.Status, sync.OneCDocumentNumber, sync.AcceptedAtUtc)))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null ? Result.NotFound() : Result.Success(detail);
    }
}
