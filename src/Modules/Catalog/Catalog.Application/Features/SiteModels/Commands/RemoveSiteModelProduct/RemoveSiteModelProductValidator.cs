namespace Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;

public sealed class RemoveSiteModelProductValidator : AbstractValidator<RemoveSiteModelProductCommand>
{
    public RemoveSiteModelProductValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.ProductId).GreaterThan(0);
    }
}
