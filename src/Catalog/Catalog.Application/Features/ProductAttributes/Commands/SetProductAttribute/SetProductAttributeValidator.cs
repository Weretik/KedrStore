using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.SetProductAttribute;

public sealed class SetProductAttributeValidator : AbstractValidator<SetProductAttributeCommand>
{
    public SetProductAttributeValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.AttributeId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Values).NotEmpty()
            .WithMessage("Вкажіть хоча б одне значення; щоб прибрати ручні значення, натисніть «Повернути автоматичне».")
            .Must(values => values.Count <= ProductAttributeValue.MaxPosition + 1)
            .WithMessage($"Не більше {ProductAttributeValue.MaxPosition + 1} значень.")
            .When(command => command.Request is not null).OverridePropertyName("values");
        RuleForEach(command => command.Request.Values)
            .Must(value => value is not null && (value.ValueId is null) != (value.Number is null))
            .WithMessage("Для кожного значення вкажіть варіант або число.")
            .Must(value => value?.ValueId is null or > 0)
            .WithMessage("Некоректний варіант значення.")
            .When(command => command.Request?.Values is not null).OverridePropertyName("values");
    }
}
