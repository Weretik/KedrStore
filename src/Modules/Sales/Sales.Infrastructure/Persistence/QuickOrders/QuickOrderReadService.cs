using BuildingBlocks.Application.Text;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderById;
using Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;
using Sales.Domain.QuickOrders;

namespace Sales.Infrastructure.Persistence.QuickOrders;

/// <summary>
/// Admin reads of site orders as projections, paged in SQL (spec sales/009, R-006 and R-007), with the order made from each
/// for 1C looked up by its primary key (R-015).
/// </summary>
internal sealed class QuickOrderReadService(SalesDbContext dbContext) : IQuickOrderReadService
{
    public async Task<PagedResult<List<QuickOrderListItemDto>>> GetListAsync(
        QuickOrderListRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<QuickOrder>().AsNoTracking();

        // Database rules 2.1: every word must match the name or the phone; trigram indexes serve both. A word that is an
        // order number (or a whole id) also matches the ids it names, as a range on the primary key (spec 009, R-012).
        foreach (var word in SearchWords.Words(request.Search))
        {
            var pattern = "%" + SearchWords.Escape(word) + "%";
            if (QuickOrderNumberSearch.RangeOf(word) is var (fromValue, toValue))
            {
                var from = QuickOrderId.Bound(fromValue);
                var to = QuickOrderId.Bound(toValue);
                query = query.Where(order =>
                    EF.Functions.ILike(order.FirstName, pattern, SearchWords.EscapeCharacter) ||
                    EF.Functions.ILike(order.Phone, pattern, SearchWords.EscapeCharacter) ||
                    (order.Id >= from && order.Id <= to));
            }
            else
                query = query.Where(order =>
                    EF.Functions.ILike(order.FirstName, pattern, SearchWords.EscapeCharacter) ||
                    EF.Functions.ILike(order.Phone, pattern, SearchWords.EscapeCharacter));
        }

        // The same condition as the partial index IX_QuickOrders_Problem.
        if (request.Delivery == QuickOrderListRequest.ProblemDelivery)
            query = query.Where(order => order.TelegramSentAtUtc == null
                                         && (order.TelegramLastError != null || order.TelegramFailedAtUtc != null));

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(order => order.PlacedAtUtc)
            .ThenByDescending(order => order.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(order => new QuickOrderListItemDto(
                order.Id.Value,
                order.PlacedAtUtc,
                order.FirstName,
                order.Phone,
                order.Items.Count,
                order.Total,
                order.TelegramSentAtUtc != null
                    ? QuickOrderTelegramStatus.Sent
                    : order.TelegramFailedAtUtc != null ? QuickOrderTelegramStatus.Failed : QuickOrderTelegramStatus.Pending,
                order.TelegramSentAtUtc == null ? order.TelegramLastError : null,
                order.OneCOrderId == null ? null : order.OneCOrderId.Value.Value,
                dbContext.Orders.Where(made => made.Id == order.OneCOrderId).Select(made => made.OrderNumber).FirstOrDefault(),
                dbContext.Orders.Where(made => made.Id == order.OneCOrderId).Select(made => made.OneCDocumentNumber).FirstOrDefault()))
            .ToListAsync(cancellationToken);
        var totalPages = totalRecords == 0 ? 0 : (long)Math.Ceiling(totalRecords / (double)request.PageSize);

        return new PagedResult<List<QuickOrderListItemDto>>(
            new PagedInfo(request.Page, request.PageSize, totalPages, totalRecords), rows);
    }

    public Task<QuickOrderDetailDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var id = QuickOrderId.Create(orderId);
        return dbContext.Set<QuickOrder>()
            .AsNoTracking()
            .Where(order => order.Id == id)
            .Select(order => new QuickOrderDetailDto(
                order.Id.Value,
                order.PlacedAtUtc,
                order.FirstName,
                order.Phone,
                order.Total,
                order.TelegramSentAtUtc != null
                    ? QuickOrderTelegramStatus.Sent
                    : order.TelegramFailedAtUtc != null ? QuickOrderTelegramStatus.Failed : QuickOrderTelegramStatus.Pending,
                order.TelegramSentAtUtc,
                order.TelegramLastAttemptAtUtc,
                order.TelegramSentAtUtc == null ? order.TelegramLastError : null,
                order.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new QuickOrderItemDto(
                        item.ProductId, item.Title, item.UnitPrice, item.Quantity, item.UnitPrice * item.Quantity))
                    .ToList(),
                order.Comment,
                order.OneCOrderId == null ? null : order.OneCOrderId.Value.Value,
                dbContext.Orders.Where(made => made.Id == order.OneCOrderId).Select(made => made.OrderNumber).FirstOrDefault(),
                dbContext.Orders.Where(made => made.Id == order.OneCOrderId).Select(made => made.OneCDocumentNumber).FirstOrDefault()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
