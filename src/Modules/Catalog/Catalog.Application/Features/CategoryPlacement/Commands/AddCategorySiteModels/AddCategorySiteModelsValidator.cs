namespace Catalog.Application.Features.CategoryPlacement.Commands.AddCategorySiteModels;

public sealed class AddCategorySiteModelsValidator : AbstractValidator<AddCategorySiteModelsCommand>
{
    public AddCategorySiteModelsValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.ModelIds).NotEmpty().WithMessage("Оберіть хоча б одну модель.")
            .Must(ids => ids.Count <= 200).WithMessage("Не більше 200 моделей за раз.");
        RuleForEach(command => command.ModelIds).GreaterThan(0);
    }
}
