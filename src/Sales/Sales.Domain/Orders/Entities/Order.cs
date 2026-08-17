using Sales.Domain.Orders.Errors;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class Order : BaseAuditableEntity<OrderId>, IAggregateRoot, IAuditableEntity
{
    private readonly List<OrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public string CounterpartyId { get; private set; } = null!;
    public string? Comment { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { }

    private Order(string orderNumber, string counterpartyId, string? comment, IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
    {
        OrderNumber = Required(orderNumber, OrderErrors.OrderNumberRequired());
        if (OrderNumber.Length > 32)
            throw new DomainException(OrderErrors.OrderNumberTooLong());
        CounterpartyId = Required(counterpartyId, OrderErrors.CounterpartyIdRequired());
        Comment = NormalizeComment(comment);
        _lines.AddRange(lines);

        if (_lines.Count == 0)
            throw new DomainException(OrderErrors.LinesRequired());

        MarkAsCreated(createdAt);
    }

    public static Order Create(string orderNumber, string counterpartyId, string? comment, IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
        => new(orderNumber, counterpartyId, comment, lines, createdAt);

    private static string Required(string value, IDomainError error)
        => string.IsNullOrWhiteSpace(value) ? throw new DomainException(error) : value.Trim();

    private static string? NormalizeComment(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        if (trimmed.Length > 1_000)
            throw new DomainException(OrderErrors.CommentTooLong());

        return trimmed;
    }
}
