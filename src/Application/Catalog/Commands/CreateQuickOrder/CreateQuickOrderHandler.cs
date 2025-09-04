namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed class CreateQuickOrderCommandHandler(ITelegramNotifier telegram)
    : ICommandHandler<CreateQuickOrderCommand, Result>
{
    public async ValueTask<Result> Handle(CreateQuickOrderCommand request, CancellationToken cancellationToken)
    {
        var name  = request.Name?.Trim();

        var phone = PhoneNumberHelper.TryParse(request.Phone, out var e164)
            ? e164!
            : request.Phone;

        var message = string.Join(Environment.NewLine, new[]
        {
            "Нова заявка:",
            $"Ім'я: {name}",
            $"Телефон: {phone}"
        });

        await telegram.SendMessageAsync(message, cancellationToken);
        return Result.Success();
    }
}
