using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Rename;

public sealed class RenameSiteModelCommandValidator : AbstractValidator<RenameSiteModelCommand>
{
    public RenameSiteModelCommandValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("name");
    }
}
