namespace Sales.Domain.Orders.Errors;

public static class OrderErrors
{
    public static IDomainError CounterpartyIdRequired() => new DomainError("sales.order.counterparty_id_required", "Counterparty id is required.");
    public static IDomainError LinesRequired() => new DomainError("sales.order.lines_required", "At least one order line is required.");
    public static IDomainError ProductIdRequired() => new DomainError("sales.order.product_id_required", "Product id is required.");
    public static IDomainError ProductNameRequired() => new DomainError("sales.order.product_name_required", "Product name is required.");
    public static IDomainError QuantityInvalid() => new DomainError("sales.order.quantity_invalid", "Quantity must be positive.");
    public static IDomainError AmountInvalid() => new DomainError("sales.order.amount_invalid", "Line amount must be non-negative.");
    public static IDomainError CommentTooLong() => new DomainError("sales.order.comment_too_long", "Order comment is too long.");
    public static IDomainError OrderNumberRequired() => new DomainError("sales.order.order_number_required", "Order number is required.");
    public static IDomainError OrderNumberTooLong() => new DomainError("sales.order.order_number_too_long", "Order number is too long.");
    public static IDomainError IdempotencyKeyRequired() => new DomainError("sales.order.idempotency_key_required", "Idempotency key is required.");
    public static IDomainError RequestHashRequired() => new DomainError("sales.order.request_hash_required", "Request hash is required.");
}
