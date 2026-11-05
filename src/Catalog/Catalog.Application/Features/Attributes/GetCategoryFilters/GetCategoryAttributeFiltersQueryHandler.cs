using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.GetCategoryFilters;

public sealed class GetCategoryAttributeFiltersQueryHandler(CategoryFilterReader reader)
    : IQueryHandler<GetCategoryAttributeFiltersQuery, Result<List<AdminCategoryAttributeFilterDto>>>
{
    public async ValueTask<Result<List<AdminCategoryAttributeFilterDto>>> Handle(GetCategoryAttributeFiltersQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductCategoryId.From(query.CategoryId), cancellationToken) is { } filters
            ? Result.Success(filters)
            : Result.NotFound();
}
