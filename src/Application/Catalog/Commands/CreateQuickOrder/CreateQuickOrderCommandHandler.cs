namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed class CreateQuickOrderCommandHandler(ITelegramNotifier telegram)
    : ICommandHandler<CreateQuickOrderCommand, Result>
{
    public async ValueTask<Result> Handle(CreateQuickOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name  = request.Request.Name.Trim();

        var phone = PhoneNumberHelper.TryParse(request.Request.Phone, out var e164)
            ? e164!
            : request.Request.Phone;

        var message = string.IsNullOrWhiteSpace(request.Request.Message)
            ? string.Empty
            : request.Request.Message.Trim() ;

        var parts = new List<string>
        {
            "<b>Нове звернення:</b>",
            $"👨‍💼 {name}",
            $"📲 {phone}"
        };
        if (!string.IsNullOrWhiteSpace(message))
        {
            parts.Add("");
            parts.Add("<b>Повідомлення:</b>");
            parts.Add(message);
        }
        var messageText = string.Join(Environment.NewLine, parts);

        await telegram.SendMessageAsync(messageText, cancellationToken);
        return Result.Success();
    }
}
