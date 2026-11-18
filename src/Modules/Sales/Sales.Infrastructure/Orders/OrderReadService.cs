using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.Queries.GetOrderById;
using Sales.Application.Features.Orders.Queries.GetOrderList;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderReadService(SalesDbContext dbContext) : IOrderReadService
{
    public async Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
        OrderListRequest request,
        CancellationToken cancellationToken)
    {
        var query =
            from row in dbContext.OrdersWithOneCMessage()
            join counterparty in dbContext.Counterparties.IgnoreQueryFilters().AsNoTracking()
                on row.Order.CounterpartyId equals counterparty.Id
            where request.CounterpartyId == null || row.Order.CounterpartyId == CounterpartyId.FromStorage(request.CounterpartyId)
            orderby row.Order.CreatedAt descending, row.Order.Id descending
            select new
            {
                row.Order.Id,
                row.Order.OrderNumber,
                row.Order.CounterpartyId,
                counterparty.Name,
                row.Order.CreatedAt,
                LineCount = row.Order.Lines.Count,
                Total = row.Order.Lines.Sum(line => line.Amount),
                row.Order.OneCDocumentNumber,
                row.Order.OneCRejection,
                row.Message
            };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = (await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken))
            .Select(x => new OrderListItemDto(x.Id.Value, x.OrderNumber, x.CounterpartyId.Value, x.Name, x.CreatedAt,
                x.LineCount, x.Total, OrderOneCQueries.OneC(x.OneCDocumentNumber, x.OneCRejection, x.Message).Status,
                x.OneCDocumentNumber))
            .ToList();
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
        var row = await (
            from x in dbContext.OrdersWithOneCMessage()
            join counterparty in dbContext.Counterparties.IgnoreQueryFilters().AsNoTracking()
                on x.Order.CounterpartyId equals counterparty.Id
            where x.Order.Id == storageId
            let order = x.Order
            select new { x.Message, Detail = new OrderDetailDto(
                order.Id.Value,
                order.OrderNumber,
                order.CreatedAt,
                new OrderCounterpartyDto(counterparty.Id.Value, counterparty.Name, counterparty.Phone),
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
                new OrderSyncDto(OneCOrderSyncStatus.Pending, order.OneCDocumentNumber, order.OneCAcceptedAtUtc)),
                order.OneCRejection })
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null)
            return Result.NotFound();

        var status = OrderOneCQueries.OneC(row.Detail.Sync.OneCDocumentNumber, row.OneCRejection, row.Message).Status;
        return Result.Success(row.Detail with { Sync = row.Detail.Sync with { Status = status } });
    }
}
