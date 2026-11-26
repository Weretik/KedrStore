namespace Catalog.Application.Features.CategoryPlacement.Commands.DeleteManualCategory;

public sealed class DeleteManualCategoryValidator : AbstractValidator<DeleteManualCategoryCommand>
{
    public DeleteManualCategoryValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
    }
}
