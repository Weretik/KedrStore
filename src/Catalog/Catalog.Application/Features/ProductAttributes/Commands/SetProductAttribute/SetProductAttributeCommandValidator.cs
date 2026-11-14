using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Commands.SetProductAttribute;

public sealed class SetProductAttributeCommandValidator : AbstractValidator<SetProductAttributeCommand>
{
    public SetProductAttributeCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.AttributeId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Values).NotEmpty()
            .WithMessage("Set at least one value; use «Повернути автоматичне» to drop manual values.")
            .Must(values => values.Count <= ProductAttributeValue.MaxPosition + 1)
            .WithMessage($"At most {ProductAttributeValue.MaxPosition + 1} values.")
            .When(command => command.Request is not null).OverridePropertyName("values");
        RuleForEach(command => command.Request.Values)
            .Must(value => value is not null && (value.ValueId is null) != (value.Number is null))
            .WithMessage("Each value needs either valueId or number.")
            .Must(value => value?.ValueId is null or > 0)
            .WithMessage("valueId must be positive.")
            .When(command => command.Request?.Values is not null).OverridePropertyName("values");
    }
}
