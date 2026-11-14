namespace Catalog.Application.Features.SiteModels.Commands.FillSiteModelFromNames;

public sealed class FillSiteModelFromNamesValidator : AbstractValidator<FillSiteModelFromNamesCommand>
{
    public FillSiteModelFromNamesValidator() => RuleFor(command => command.ModelId).GreaterThan(0);
}
