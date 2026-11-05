namespace Catalog.Application.Features.Attributes.SetCategoryFilters;

public sealed class SetCategoryAttributeFiltersCommandValidator : AbstractValidator<SetCategoryAttributeFiltersCommand>
{
    public const int MaxFilters = 100;

    public SetCategoryAttributeFiltersCommandValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Filters).NotNull()
            .Must(filters => filters.Count <= MaxFilters).WithMessage($"At most {MaxFilters} filters.")
            .Must(filters => filters.Select(filter => filter.AttributeId).Distinct().Count() == filters.Count)
            .WithMessage("Each attribute once.")
            .When(command => command.Request is not null).OverridePropertyName("filters");
        RuleForEach(command => command.Request.Filters)
            .Must(filter => filter is not null && filter.AttributeId > 0 && filter.SortOrder is >= 0 and <= 100_000)
            .WithMessage("Each filter needs a positive attributeId and a sortOrder from 0 to 100000.")
            .When(command => command.Request?.Filters is not null).OverridePropertyName("filters");
    }
}
