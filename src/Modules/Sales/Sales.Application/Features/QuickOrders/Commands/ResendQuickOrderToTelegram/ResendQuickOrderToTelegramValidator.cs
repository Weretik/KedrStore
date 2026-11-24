namespace Sales.Application.Features.QuickOrders.Commands.ResendQuickOrderToTelegram;

public sealed class ResendQuickOrderToTelegramValidator : AbstractValidator<ResendQuickOrderToTelegramCommand>
{
    public ResendQuickOrderToTelegramValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEqual(Guid.Empty).WithMessage("Некоректний номер замовлення.");
    }
}
