using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;

public sealed class SetCategorySiteVisibilityCommandValidator : AbstractValidator<SetCategorySiteVisibilityCommand>
{
    public SetCategorySiteVisibilityCommandValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
    }
}
