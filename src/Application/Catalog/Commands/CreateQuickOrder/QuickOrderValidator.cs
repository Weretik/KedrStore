using Application.Common.Helpers;

namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed class QuickOrderValidator : AbstractValidator<IQuickOrder>
{
    public QuickOrderValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Ім'я обов'язково")
            .MinimumLength(3).WithMessage("Занадто коротке ім'я")
            .MaximumLength(25).WithMessage("Занадто довге ім'я");

        RuleFor(request => request.Phone)
            .NotEmpty().WithMessage("Телефон обов'язковий")
            .Must(parameter => PhoneNumberHelper.TryParse(parameter, out _))
            .WithMessage("Некоректний номер телефону");

        RuleFor(request => request.Message)
            .MaximumLength(500).WithMessage("Повідомлення завелике");
    }
}
