using Microsoft.Extensions.Logging;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Commands.ResendQuickOrderToTelegram;

/// <summary>
/// Puts a failed site order back in the Telegram queue (spec sales/009, R-008). Idempotency by state check: only a
/// failed order can be resent, so a repeat finds it pending and answers 409 without a second message; two parallel
/// resends are told apart by the row version.
/// </summary>
public sealed class ResendQuickOrderToTelegramHandler(
    IQuickOrderStore store,
    ILogger<ResendQuickOrderToTelegramHandler> logger)
    : ICommandHandler<ResendQuickOrderToTelegramCommand, Result<ResendQuickOrderToTelegramResult>>
{
    public async ValueTask<Result<ResendQuickOrderToTelegramResult>> Handle(
        ResendQuickOrderToTelegramCommand command,
        CancellationToken cancellationToken)
    {
        var order = await store.FindAsync(QuickOrderId.Create(command.OrderId), cancellationToken);
        if (order is null)
            return QuickOrderErrors.NotFound().ToResult<ResendQuickOrderToTelegramResult>();

        var resend = order.RequestTelegramResend();
        if (resend.IsFailure)
            return resend.Error!.ToResult<ResendQuickOrderToTelegramResult>();

        if (!await store.TrySaveResendAsync(order, QuickOrderPlaced.From(order), cancellationToken))
            return QuickOrderErrors.ChangedMeanwhile().ToResult<ResendQuickOrderToTelegramResult>();

        logger.LogInformation("Quick order {QuickOrderId} resent to Telegram by {UserId}.", command.OrderId, command.RequestedBy);
        return Result.Success(new ResendQuickOrderToTelegramResult(order.Id.Value, QuickOrderTelegramStatus.Pending));
    }
}
