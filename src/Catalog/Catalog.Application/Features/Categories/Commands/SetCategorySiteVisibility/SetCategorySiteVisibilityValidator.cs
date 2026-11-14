using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;

public sealed class SetCategorySiteVisibilityValidator : AbstractValidator<SetCategorySiteVisibilityCommand>
{
    public SetCategorySiteVisibilityValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
    }
}
