namespace Catalog.Application.Features.Orders.Commands.CreateQuickOrder;

public interface ITelegramNotifier
{
    Task SendMessageAsync(string text, CancellationToken ct = default);
}
