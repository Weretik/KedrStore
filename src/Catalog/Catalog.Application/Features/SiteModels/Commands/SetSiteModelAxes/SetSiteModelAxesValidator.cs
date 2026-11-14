namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;

public sealed class SetSiteModelAxesValidator : AbstractValidator<SetSiteModelAxesCommand>
{
    public SetSiteModelAxesValidator()
    {
        RuleFor(command => command.ModelId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.AttributeIds).NotNull()
            .Must(ids => ids.Count <= SetSiteModelAxesCommand.MaxAxes)
            .WithMessage($"У моделі не більше {SetSiteModelAxesCommand.MaxAxes} осей.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Ідентифікатори характеристик мають бути додатні й без повторів.")
            .When(command => command.Request is not null).OverridePropertyName("attributeIds");
    }
}
