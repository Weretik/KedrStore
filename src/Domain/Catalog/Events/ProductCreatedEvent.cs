namespace Domain.Catalog.Events;

public sealed class ProductCreatedEvent(ProductId productId) : BaseDomainEvent
{
    public ProductId ProductId { get; } = productId;
}
