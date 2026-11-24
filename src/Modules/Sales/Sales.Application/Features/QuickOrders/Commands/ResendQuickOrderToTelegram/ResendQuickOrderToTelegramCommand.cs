namespace Sales.Application.Features.QuickOrders.Commands.ResendQuickOrderToTelegram;

/// <summary>A person sends a site order whose Telegram delivery failed once more (spec sales/009, R-008).</summary>
public sealed record ResendQuickOrderToTelegramCommand(Guid OrderId, string RequestedBy)
    : ICommand<Result<ResendQuickOrderToTelegramResult>>;
