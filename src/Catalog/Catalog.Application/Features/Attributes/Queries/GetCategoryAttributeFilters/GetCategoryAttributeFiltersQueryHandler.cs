using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;

public sealed class GetCategoryAttributeFiltersQueryHandler(CategoryFilterReader reader)
    : IQueryHandler<GetCategoryAttributeFiltersQuery, Result<List<AdminCategoryAttributeFilterDto>>>
{
    public async ValueTask<Result<List<AdminCategoryAttributeFilterDto>>> Handle(GetCategoryAttributeFiltersQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductCategoryId.From(query.CategoryId), cancellationToken) is { } filters
            ? Result.Success(filters)
            : Result.NotFound();
}
