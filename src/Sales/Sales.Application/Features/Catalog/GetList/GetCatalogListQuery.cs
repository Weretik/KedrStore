namespace Sales.Application.Features.Catalog.GetList;

public sealed record GetCatalogListQuery(CatalogRequest Request)
    : IQuery<Result<PagedResult<List<CatalogListItemDto>>>>;
