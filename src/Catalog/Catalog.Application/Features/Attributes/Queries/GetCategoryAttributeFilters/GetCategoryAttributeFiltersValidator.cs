namespace Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;

public sealed class GetCategoryAttributeFiltersValidator : AbstractValidator<GetCategoryAttributeFiltersQuery>
{
    public GetCategoryAttributeFiltersValidator()
    {
        RuleFor(query => query.CategoryId).GreaterThan(0);
    }
}
