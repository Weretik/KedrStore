namespace Sales.Api.Endpoints.QuickOrders.ResendQuickOrderToTelegram;

/// <param name="TelegramStatus">Always <c>pending</c>: the order is back in the queue.</param>
public sealed record ResendAdminQuickOrderToTelegramResponse(Guid OrderId, string TelegramStatus);
