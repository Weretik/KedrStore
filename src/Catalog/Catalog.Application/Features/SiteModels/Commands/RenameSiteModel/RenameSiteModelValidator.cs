using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Commands.RenameSiteModel;

public sealed class RenameSiteModelValidator : AbstractValidator<RenameSiteModelCommand>
{
    public RenameSiteModelValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("name");
    }
}
