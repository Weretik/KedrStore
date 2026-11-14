namespace Catalog.Application.Features.Groups.Commands.SetProductGroupMain;

public sealed class SetProductGroupMainValidator : AbstractValidator<SetProductGroupMainCommand>
{
    public SetProductGroupMainValidator()
    {
        RuleFor(command => command.GroupId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ProductId).GreaterThan(0)
            .When(command => command.Request is not null).OverridePropertyName("productId");
    }
}
