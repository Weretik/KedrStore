namespace Catalog.Application.Features.Category.SiteVisibility;

public sealed class SetCategorySiteVisibilityCommandValidator : AbstractValidator<SetCategorySiteVisibilityCommand>
{
    public SetCategorySiteVisibilityCommandValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
    }
}
