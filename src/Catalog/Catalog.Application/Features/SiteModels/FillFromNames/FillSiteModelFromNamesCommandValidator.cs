namespace Catalog.Application.Features.SiteModels.FillFromNames;

public sealed class FillSiteModelFromNamesCommandValidator : AbstractValidator<FillSiteModelFromNamesCommand>
{
    public FillSiteModelFromNamesCommandValidator() => RuleFor(command => command.ModelId).GreaterThan(0);
}
