namespace Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;

public sealed class AcceptSiteModelsValidator : AbstractValidator<AcceptSiteModelsCommand>
{
    public AcceptSiteModelsValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Ids).NotEmpty()
            .Must(ids => ids.Count <= AcceptSiteModelsCommand.MaxModels)
            .WithMessage($"At most {AcceptSiteModelsCommand.MaxModels} models at once.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Model IDs must be positive and distinct.")
            .When(command => command.Request is not null).OverridePropertyName("ids");
    }
}
