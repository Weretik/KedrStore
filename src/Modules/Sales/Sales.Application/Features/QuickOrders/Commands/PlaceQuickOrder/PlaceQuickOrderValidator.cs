using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

public sealed class PlaceQuickOrderValidator : AbstractValidator<PlaceQuickOrderCommand>
{
    public PlaceQuickOrderValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Вкажіть ім’я.")
            .MaximumLength(QuickOrder.FirstNameMaxLength).WithMessage("Ім’я надто довге (не більше 30 символів).")
            .WithName("Ім’я");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Вкажіть номер телефону.")
            .MaximumLength(QuickOrder.PhoneMaxLength).WithMessage("Номер телефону надто довгий.")
            .WithName("Телефон");

        RuleFor(x => x.Comment)
            .MaximumLength(QuickOrder.CommentMaxLength).WithMessage("Коментар надто довгий (не більше 800 символів).")
            .WithName("Коментар");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Додайте хоча б один товар.")
            .WithName("Товари");

        RuleFor(x => x.IdempotencyKey)
            .NotEqual(Guid.Empty).WithMessage("Некоректний ключ повтору запиту.")
            .WithName("Idempotency-Key");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Некоректний товар.")
                .MaximumLength(QuickOrderItem.ProductIdMaxLength).WithMessage("Некоректний товар.");

            line.RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Вкажіть назву товару.")
                .MaximumLength(QuickOrderItem.TitleMaxLength).WithMessage("Назва товару надто довга.");

            line.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0m).WithMessage("Ціна не може бути від'ємною.")
                .WithName("Ціна");

            line.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Кількість має бути більшою за 0.")
                .LessThanOrEqualTo(1000).WithMessage("Максимальна кількість — 1000.")
                .WithName("Кількість");
        });
    }
}
