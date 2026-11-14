using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.CreateSiteModel;

public sealed class CreateSiteModelValidator : AbstractValidator<CreateSiteModelCommand>
{
    public CreateSiteModelValidator()
    {
        RuleFor(command => command.Request).NotNull();
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.CategoryId).GreaterThan(0).OverridePropertyName("categoryId");
            RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
                .OverridePropertyName("name");
            RuleFor(command => command.Request.ProductIds).NotNull()
                .Must(ids => ids.Count <= CreateSiteModelCommand.MaxProducts)
                .WithMessage($"У нову модель можна додати не більше {CreateSiteModelCommand.MaxProducts} товарів.")
                .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
                .WithMessage("Ідентифікатори товарів мають бути додатні й без повторів.")
                .OverridePropertyName("productIds");
        });
    }
}
