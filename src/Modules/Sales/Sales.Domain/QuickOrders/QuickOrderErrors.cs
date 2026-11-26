using BuildingBlocks.Domain.Abstractions.Errors;

namespace Sales.Domain.QuickOrders;

public static class QuickOrderErrors
{
    public static DomainError FirstNameRequired() => new("sales.quick_order.first_name_required", "Вкажіть ім’я.", Field: "FirstName");
    public static DomainError PhoneRequired() => new("sales.quick_order.phone_required", "Вкажіть номер телефону.", Field: "Phone");
    public static DomainError ItemsRequired() => new("sales.quick_order.items_required", "Додайте хоча б один товар.", Field: "Lines");
    public static DomainError ProductIdRequired() => new("sales.quick_order.product_id_required", "Некоректний товар.", Field: "ProductId");
    public static DomainError TitleRequired() => new("sales.quick_order.title_required", "Вкажіть назву товару.", Field: "Title");
    public static DomainError QuantityInvalid() => new("sales.quick_order.quantity_invalid", "Кількість має бути більшою за 0.", Field: "Quantity");
    public static DomainError UnitPriceInvalid() => new("sales.quick_order.unit_price_invalid", "Ціна не може бути від'ємною.", Field: "UnitPrice");

    /// <summary>The same <c>Idempotency-Key</c> came with other data: the site made one key for two orders.</summary>
    public static DomainError IdempotencyConflict() =>
        DomainError.Conflict("idempotency_conflict", "Це замовлення вже надіслано з іншими даними. Оновіть сторінку.");

    public static DomainError NotFound() => DomainError.NotFound("not_found", "Замовлення не знайдено.");
    public static DomainError AlreadySent() => DomainError.Conflict("already_sent", "Замовлення вже надіслано в Telegram.");
    public static DomainError DeliveryInProgress() =>
        DomainError.Conflict("delivery_in_progress", "Замовлення ще відправляється в Telegram. Спробуйте пізніше.");
    public static DomainError ProductNotInCatalog(string productId) =>
        new("sales.quick_order.product_not_in_catalog", $"Товару {productId} немає в каталозі.", Field: "ProductId");
    public static DomainError OneCOrderAlreadyLinked() =>
        DomainError.Conflict("one_c_order_already_linked", "Для цього замовлення вже створено замовлення для 1С.");
    public static DomainError ChangedMeanwhile() =>
        DomainError.Conflict("concurrency_conflict", "Стан замовлення змінився. Оновіть сторінку й спробуйте ще раз.");
}
