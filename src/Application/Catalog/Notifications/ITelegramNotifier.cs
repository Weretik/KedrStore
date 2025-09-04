namespace Application.Catalog.Notifications;

public interface ITelegramNotifier
{
    Task SendMessageAsync(string text, CancellationToken ct = default);
}
