using BuildingBlocks.Domain.Abstractions.Errors;

namespace Sales.Application.Features.OrderWorkspace.Shared;

/// <summary>The order workspace's answers that depend on stored data rather than on the request alone.</summary>
public static class WorkspaceErrors
{
    public static DomainError CustomerNotFound() =>
        DomainError.NotFound("customer_not_found", "Клієнта не знайдено. Оберіть клієнта ще раз.");

    public static DomainError ProductNotFound() =>
        DomainError.NotFound("product_not_found", "Одного з товарів більше немає в каталозі. Приберіть його з замовлення.");

    public static DomainError DraftNotFound() =>
        DomainError.NotFound("draft_not_found", "Чернетку не знайдено: можливо, її вже видалено.");

    public static DomainError VersionConflict() =>
        DomainError.Conflict("version_conflict", "Чернетку вже змінили. Оновіть сторінку.");

    public static DomainError IdempotencyConflict() =>
        DomainError.Conflict("idempotency_conflict", "Цей запит уже надіслано з іншими даними. Оновіть сторінку.");

    public static DomainError DraftDeleted() =>
        DomainError.Conflict("draft_deleted", "Чернетку, створену цим запитом, уже видалено.");

    /// <summary>A stored draft whose lines no longer pass the input rules; new input is checked by validators.</summary>
    public static DomainError InvalidLines() =>
        new("invalid_lines", WorkspaceInputRules.LinesMessage);
}
