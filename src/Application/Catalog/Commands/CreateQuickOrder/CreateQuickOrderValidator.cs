namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed class CreateQuickOrderValidator : AbstractValidator<CreateQuickOrderCommand>
{
    private static readonly PhoneNumberUtil _phone = PhoneNumberUtil.GetInstance();

    public CreateQuickOrderValidator()
    {
        RuleFor(q => q.Name)
            .NotEmpty().WithMessage("Ім'я обов'язково")
            .MinimumLength(2).WithMessage("Занадто коротке ім'я")
            .MaximumLength(80).WithMessage("Занадто довге ім'я");

        RuleFor(q => q.Phone)
            .NotEmpty().WithMessage("Телефон обов'язковий")
            .Must(p => PhoneNumberHelper.TryParse(p, out _))
            .WithMessage("Некоректний номер телефону");
    }
}
