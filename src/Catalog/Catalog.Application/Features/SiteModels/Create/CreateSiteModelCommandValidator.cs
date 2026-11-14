using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Create;

public sealed class CreateSiteModelCommandValidator : AbstractValidator<CreateSiteModelCommand>
{
    public CreateSiteModelCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.CategoryId).GreaterThan(0).OverridePropertyName("categoryId");
            RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
                .OverridePropertyName("name");
            RuleFor(command => command.Request.ProductIds).NotNull()
                .Must(ids => ids.Count <= CreateSiteModelCommand.MaxProducts)
                .WithMessage($"A new model can take at most {CreateSiteModelCommand.MaxProducts} products.")
                .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
                .WithMessage("Product IDs must be positive and distinct.")
                .OverridePropertyName("productIds");
        });
    }
}
