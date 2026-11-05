namespace Catalog.Application.Features.Attributes.DecideCode;

public sealed class DecideAttributeCodeCommandValidator : AbstractValidator<DecideAttributeCodeCommand>
{
    public DecideAttributeCodeCommandValidator()
    {
        RuleFor(command => command.AliasId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ValueId).GreaterThan(0)
            .When(command => command.Request?.ValueId is not null).OverridePropertyName("valueId");
    }
}
