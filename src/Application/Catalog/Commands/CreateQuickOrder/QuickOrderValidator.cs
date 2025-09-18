namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed class QuickOrderValidator : AbstractValidator<IQuickOrder>
{
    public QuickOrderValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Ім'я обов'язково")
            .MinimumLength(2).WithMessage("Занадто коротке ім'я")
            .MaximumLength(80).WithMessage("Занадто довге ім'я");

        RuleFor(request => request.Phone)
            .NotEmpty().WithMessage("Телефон обов'язковий")
            .Must(parameter => PhoneNumberHelper.TryParse(parameter, out _))
            .WithMessage("Некоректний номер телефону");

        RuleFor(request => request.Message)
            .MaximumLength(500).WithMessage("Повідомлення завелике");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<IQuickOrder>.CreateWithOptions((IQuickOrder)model,
                x => x.IncludeProperties(propertyName)));

        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}
