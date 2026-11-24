using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.QuickOrders.Commands.ResendQuickOrderToTelegram;
namespace Sales.Api.Endpoints.QuickOrders.ResendQuickOrderToTelegram;

internal static class ResendQuickOrderToTelegramEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{orderId:guid}/telegram-resend", Handle)
            .WithName("resendAdminQuickOrderToTelegram")
            .RequireRateLimiting("HeavyOperation")
            .WithSummary("Send a site order to Telegram again after its delivery failed")
            .WithDescription("Allowed only when the delivery failed (telegramStatus failed): the order goes back in the outbox queue with attempts from zero; Telegram is not called in the request. A sent order answers 409 already_sent, an order still being sent 409 delivery_in_progress, so a repeat sends nothing twice. Policy CanManageOrders.")
            .Produces<ResendAdminQuickOrderToTelegramResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId, ClaimsPrincipal user, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var requestedBy = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name ?? "unknown";
        var result = await sender.Send(new ResendQuickOrderToTelegramCommand(orderId, requestedBy), cancellationToken);

        return result.Status == ResultStatus.Ok
            ? TypedResults.Json(
                new ResendAdminQuickOrderToTelegramResponse(
                    result.Value.OrderId, result.Value.TelegramStatus.ToString().ToLowerInvariant()),
                statusCode: StatusCodes.Status202Accepted)
            : result.ToProblem(http);
    }
}
