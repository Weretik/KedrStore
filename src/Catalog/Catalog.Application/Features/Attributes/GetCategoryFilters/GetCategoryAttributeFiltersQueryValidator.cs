namespace Catalog.Application.Features.Attributes.GetCategoryFilters;

public sealed class GetCategoryAttributeFiltersQueryValidator : AbstractValidator<GetCategoryAttributeFiltersQuery>
{
    public GetCategoryAttributeFiltersQueryValidator()
    {
        RuleFor(query => query.CategoryId).GreaterThan(0);
    }
}
