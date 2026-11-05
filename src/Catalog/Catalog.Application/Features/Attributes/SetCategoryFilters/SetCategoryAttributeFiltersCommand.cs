using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.SetCategoryFilters;

/// <summary>Admin turns category filters on or off and orders them; the attribute import then leaves them.</summary>
public sealed record SetCategoryAttributeFiltersCommand(int CategoryId, SetCategoryAttributeFiltersRequest Request)
    : ICommand<Result<List<AdminCategoryAttributeFilterDto>>>;
