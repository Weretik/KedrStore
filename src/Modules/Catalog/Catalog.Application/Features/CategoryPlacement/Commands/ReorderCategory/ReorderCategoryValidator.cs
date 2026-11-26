namespace Catalog.Application.Features.CategoryPlacement.Commands.ReorderCategory;

public sealed class ReorderCategoryValidator : AbstractValidator<ReorderCategoryCommand>
{
    public ReorderCategoryValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.Direction).Must(direction => direction is "up" or "down")
            .WithMessage("Оберіть «вище» або «нижче».");
    }
}
