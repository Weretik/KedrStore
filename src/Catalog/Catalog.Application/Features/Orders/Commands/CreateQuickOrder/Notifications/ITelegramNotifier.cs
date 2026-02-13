namespace Catalog.Application.Features.Orders.Commands.CreateQuickOrder.Notifications;

public interface ITelegramNotifier
{
    Task SendMessageAsync(string text, CancellationToken cancellationToken = default);

    Task SendDocumentAsync(
        string fileName,
        string contentType,
        byte[] bytes,
        string? caption = null,
        CancellationToken cancellationToken = default);
}
