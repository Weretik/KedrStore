namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

public sealed class PlaceQuickOrderValidator : AbstractValidator<PlaceQuickOrderCommand>
{
    public PlaceQuickOrderValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Вкажіть ім’я.")
            .MaximumLength(30).WithMessage("Ім’я надто довге (не більше 30 символів).")
            .WithName("Ім’я");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Вкажіть номер телефону.")
            .MaximumLength(20).WithMessage("Номер телефону надто довгий.")
            .WithName("Телефон");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Додайте хоча б один товар.")
            .WithName("Товари");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Некоректний товар.");

            line.RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Вкажіть назву товару.");

            line.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Кількість має бути більшою за 0.")
                .LessThanOrEqualTo(1000).WithMessage("Максимальна кількість — 1000.")
                .WithName("Кількість");
        });
    }
}
