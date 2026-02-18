namespace Catalog.Application.Features.Orders.Create.Validators;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("Вкажіть ім’я")
            .MaximumLength(30).WithMessage("Ім’я надто довге (макс. 30 символів)")
            .WithName("Ім’я");

        RuleFor(x => x.Request.Phone)
            .NotEmpty().WithMessage("Вкажіть номер телефону")
            .MaximumLength(20).WithMessage("Номер телефону надто довгий")
            .WithName("Телефон");

        RuleFor(x => x.Request.Lines)
            .NotEmpty().WithMessage("Додайте хоча б один товар")
            .WithName("Товари");

        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Некоректний товар");

            line.RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Назва товару обов’язкова");

            line.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Кількість має бути більшою за 0")
                .LessThanOrEqualTo(1000).WithMessage("Максимальна кількість — 1000")
                .WithName("Кількість");
        });
    }
}
