namespace Catalog.Application.Features.SiteModels.Commands.ChangeSiteModelCategory;

public sealed class ChangeSiteModelCategoryValidator : AbstractValidator<ChangeSiteModelCategoryCommand>
{
    public ChangeSiteModelCategoryValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.CategoryId).GreaterThan(0)
            .When(command => command.Request is not null).OverridePropertyName("categoryId");
    }
}
