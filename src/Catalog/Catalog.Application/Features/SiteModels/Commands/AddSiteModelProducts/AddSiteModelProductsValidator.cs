namespace Catalog.Application.Features.SiteModels.Commands.AddSiteModelProducts;

public sealed class AddSiteModelProductsValidator : AbstractValidator<AddSiteModelProductsCommand>
{
    public AddSiteModelProductsValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ProductIds).NotEmpty()
            .Must(ids => ids.Count <= AddSiteModelProductsCommand.MaxProducts)
            .WithMessage($"At most {AddSiteModelProductsCommand.MaxProducts} products at once.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Product IDs must be positive and distinct.")
            .When(command => command.Request is not null).OverridePropertyName("productIds");
    }
}
