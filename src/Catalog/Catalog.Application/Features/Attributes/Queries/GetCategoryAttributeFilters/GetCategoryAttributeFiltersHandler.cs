using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;

public sealed class GetCategoryAttributeFiltersHandler(CategoryFilterReader reader)
    : IQueryHandler<GetCategoryAttributeFiltersQuery, Result<List<AdminCategoryAttributeFilterDto>>>
{
    public async ValueTask<Result<List<AdminCategoryAttributeFilterDto>>> Handle(GetCategoryAttributeFiltersQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductCategoryId.From(query.CategoryId), cancellationToken) is { } filters
            ? Result.Success(filters)
            : Result.NotFound();
}
