using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Queries.GetCategoryAttributeFilters;

/// <summary>The «Фільтри категорій» of one category.</summary>
public sealed record GetCategoryAttributeFiltersQuery(int CategoryId) : IQuery<Result<List<AdminCategoryAttributeFilterDto>>>;
