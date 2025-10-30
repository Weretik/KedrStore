namespace Application.Catalog.CreateQuickOrder;

public interface ITelegramNotifier
{
    Task SendMessageAsync(string text, CancellationToken ct = default);
}
