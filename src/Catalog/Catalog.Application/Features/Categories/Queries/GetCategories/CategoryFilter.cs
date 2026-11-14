using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Queries.GetCategories;

public sealed record CategoryFilter(int? ProductTypeId = null);
