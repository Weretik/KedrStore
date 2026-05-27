namespace Sales.Application.Features.Catalog.GetList;

public sealed record GetSalesCatalogListQuery(GetSalesCatalogRequest Request)
    : IQuery<Result<PagedResult<List<SalesCatalogListItemDto>>>>;
