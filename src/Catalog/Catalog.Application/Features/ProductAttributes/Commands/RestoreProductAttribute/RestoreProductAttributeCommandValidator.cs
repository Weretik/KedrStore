namespace Catalog.Application.Features.ProductAttributes.Commands.RestoreProductAttribute;

public sealed class RestoreProductAttributeCommandValidator : AbstractValidator<RestoreProductAttributeCommand>
{
    public RestoreProductAttributeCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.AttributeId).GreaterThan(0);
    }
}
