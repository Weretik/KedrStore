namespace Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;

public sealed class RemoveSiteModelProductCommandValidator : AbstractValidator<RemoveSiteModelProductCommand>
{
    public RemoveSiteModelProductCommandValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.ProductId).GreaterThan(0);
    }
}
