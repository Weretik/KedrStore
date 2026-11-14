namespace Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;

public sealed class AcceptSiteModelsValidator : AbstractValidator<AcceptSiteModelsCommand>
{
    public AcceptSiteModelsValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Ids).NotEmpty()
            .Must(ids => ids.Count <= AcceptSiteModelsCommand.MaxModels)
            .WithMessage($"За раз не більше {AcceptSiteModelsCommand.MaxModels} моделей.")
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Ідентифікатори моделей мають бути додатні й без повторів.")
            .When(command => command.Request is not null).OverridePropertyName("ids");
    }
}
