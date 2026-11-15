using BuildingBlocks.Domain.Abstractions.Errors;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

public static class QuickOrderErrors
{
    /// <summary>The same <c>Idempotency-Key</c> came with other data: the site made one key for two orders.</summary>
    public static DomainError IdempotencyConflict() =>
        DomainError.Conflict("idempotency_conflict", "Це замовлення вже надіслано з іншими даними. Оновіть сторінку.");
}
