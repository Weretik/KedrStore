namespace Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;

public sealed class GetCategoryAttributeFiltersQueryValidator : AbstractValidator<GetCategoryAttributeFiltersQuery>
{
    public GetCategoryAttributeFiltersQueryValidator()
    {
        RuleFor(query => query.CategoryId).GreaterThan(0);
    }
}
