namespace Catalog.Application.Features.CategoryPlacement.Commands.MoveCategory;

public sealed class MoveCategoryValidator : AbstractValidator<MoveCategoryCommand>
{
    public MoveCategoryValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.ParentId).GreaterThan(0).When(command => command.ParentId.HasValue);
    }
}
