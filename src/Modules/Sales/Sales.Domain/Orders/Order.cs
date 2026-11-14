using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
using Sales.Domain.Orders.Errors;

namespace Sales.Domain.Orders;

public sealed class Order : BaseAuditableEntity<OrderId>, IAggregateRoot, IAuditableEntity
{
    public const int OrderNumberMaxLength = 32;
    public const int CommentMaxLength = 1_000;

    private readonly List<OrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public string CounterpartyId { get; private set; } = null!;
    public string? Comment { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { }

    public static DomainResult<Order> Create(string orderNumber, string counterpartyId, string? comment,
        IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return OrderErrors.OrderNumberRequired();
        if (orderNumber.Trim().Length > OrderNumberMaxLength)
            return OrderErrors.OrderNumberTooLong();
        if (string.IsNullOrWhiteSpace(counterpartyId))
            return OrderErrors.CounterpartyIdRequired();
        var normalizedComment = comment?.Trim();
        if (normalizedComment?.Length > CommentMaxLength)
            return OrderErrors.CommentTooLong();
        var orderLines = lines.ToList();
        if (orderLines.Count == 0)
            return OrderErrors.LinesRequired();

        var order = new Order
        {
            OrderNumber = orderNumber.Trim(),
            CounterpartyId = counterpartyId.Trim(),
            Comment = string.IsNullOrEmpty(normalizedComment) ? null : normalizedComment,
        };
        order._lines.AddRange(orderLines);
        order.MarkAsCreated(createdAt);
        return order;
    }

    public DomainResult AssignOrderNumber(string orderNumber)
    {
        if (!OrderNumber.StartsWith("P", StringComparison.Ordinal))
            throw new InvalidOperationException("Order number is already assigned.");
        if (string.IsNullOrWhiteSpace(orderNumber))
            return OrderErrors.OrderNumberRequired();

        OrderNumber = orderNumber.Trim();
        return DomainResult.Success();
    }
}
