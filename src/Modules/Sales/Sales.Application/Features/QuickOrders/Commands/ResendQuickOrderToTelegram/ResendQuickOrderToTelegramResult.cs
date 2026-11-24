using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Commands.ResendQuickOrderToTelegram;

public sealed record ResendQuickOrderToTelegramResult(Guid OrderId, QuickOrderTelegramStatus TelegramStatus);
