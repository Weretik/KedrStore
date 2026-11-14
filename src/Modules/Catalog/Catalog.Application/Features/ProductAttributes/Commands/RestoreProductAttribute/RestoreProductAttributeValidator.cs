namespace Catalog.Application.Features.ProductAttributes.Commands.RestoreProductAttribute;

public sealed class RestoreProductAttributeValidator : AbstractValidator<RestoreProductAttributeCommand>
{
    public RestoreProductAttributeValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.AttributeId).GreaterThan(0);
    }
}
