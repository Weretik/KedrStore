namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;

public sealed class SetSiteModelAxesCommandValidator : AbstractValidator<SetSiteModelAxesCommand>
{
    public SetSiteModelAxesCommandValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.AttributeIds).NotNull()
            .Must(ids => ids.Count <= SetSiteModelAxesCommand.MaxAxes)
            .WithMessage($"A model has at most {SetSiteModelAxesCommand.MaxAxes} axes.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Attribute IDs must be positive and distinct.")
            .When(command => command.Request is not null).OverridePropertyName("attributeIds");
    }
}
