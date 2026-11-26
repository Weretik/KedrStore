namespace Catalog.Application.Features.CategoryPlacement.Commands.CreateManualCategory;

public sealed class CreateManualCategoryValidator : AbstractValidator<CreateManualCategoryCommand>
{
    public CreateManualCategoryValidator()
    {
        RuleFor(command => command.Request.NameUk).NotEmpty().WithMessage("Заповніть назву українською.")
            .MaximumLength(ProductCategory.ShortNameMaxLength);
        RuleFor(command => command.Request.NameRu).NotEmpty().WithMessage("Заповніть назву російською.")
            .MaximumLength(ProductCategory.ShortNameMaxLength);
        RuleFor(command => command.Request.ParentId).GreaterThan(0).When(command => command.Request.ParentId.HasValue);
    }
}
