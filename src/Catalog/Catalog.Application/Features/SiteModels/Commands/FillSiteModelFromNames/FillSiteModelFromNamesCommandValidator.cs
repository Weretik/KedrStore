namespace Catalog.Application.Features.SiteModels.Commands.FillSiteModelFromNames;

public sealed class FillSiteModelFromNamesCommandValidator : AbstractValidator<FillSiteModelFromNamesCommand>
{
    public FillSiteModelFromNamesCommandValidator() => RuleFor(command => command.ModelId).GreaterThan(0);
}
