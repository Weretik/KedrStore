namespace Catalog.Application.Features.Attributes.Commands.SetCategoryAttributeFilters;

public sealed class SetCategoryAttributeFiltersValidator : AbstractValidator<SetCategoryAttributeFiltersCommand>
{
    public const int MaxFilters = 100;

    public SetCategoryAttributeFiltersValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Filters).NotNull()
            .Must(filters => filters.Count <= MaxFilters).WithMessage($"Не більше {MaxFilters} фільтрів.")
            .Must(filters => filters.Select(filter => filter.AttributeId).Distinct().Count() == filters.Count)
            .WithMessage("Кожна характеристика — лише один раз.")
            .When(command => command.Request is not null).OverridePropertyName("filters");
        RuleForEach(command => command.Request.Filters)
            .Must(filter => filter is not null && filter.AttributeId > 0 && filter.SortOrder is >= 0 and <= 100_000)
            .WithMessage("Кожному фільтру потрібна характеристика і порядок від 0 до 100000.")
            .When(command => command.Request?.Filters is not null).OverridePropertyName("filters");
    }
}
