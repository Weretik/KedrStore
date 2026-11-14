namespace Sales.Application.Features.Catalog.Queries.GetCatalogList;

public sealed record GetCatalogListQuery(CatalogRequest Request)
    : IQuery<Result<PagedResult<List<CatalogListItemDto>>>>;
