using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;

public sealed class SetSiteModelTextValidator : AbstractValidator<SetSiteModelTextCommand>
{
    public SetSiteModelTextValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Language).Must(language => ProductGroupText.Languages.Contains(language))
            .WithMessage("Мова може бути лише uk або ru.");
        RuleFor(command => command.Request).NotNull();
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.ShortDescription)
                .MaximumLength(ProductGroupText.ShortDescriptionMaxLength).OverridePropertyName("shortDescription");
            RuleFor(command => command.Request.Description)
                .MaximumLength(ProductGroupText.DescriptionMaxLength).OverridePropertyName("description");
            RuleFor(command => command.Request.SeoTitle)
                .MaximumLength(ProductGroupText.SeoTitleMaxLength).OverridePropertyName("seoTitle");
            RuleFor(command => command.Request.SeoDescription)
                .MaximumLength(ProductGroupText.SeoDescriptionMaxLength).OverridePropertyName("seoDescription");
        });
    }
}
