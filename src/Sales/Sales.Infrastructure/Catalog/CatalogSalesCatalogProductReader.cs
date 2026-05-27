namespace Sales.Infrastructure.Catalog;

internal sealed class CatalogSalesCatalogProductReader(
    ISender sender,
    ISalesCatalogPricePolicyProvider pricePolicyProvider) : ISalesCatalogProductReader
{
    public async Task<Result<PagedResult<List<SalesCatalogListItemDto>>>> GetListAsync(
        GetSalesCatalogRequest request,
        CancellationToken cancellationToken)
    {
        var pricePolicy = await pricePolicyProvider.GetPolicyAsync(request, cancellationToken);
        var catalogRequest = new GetProductsRequest
        {
            Lang = request.Lang,
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            InStock = request.InStock,
            IsSale = request.IsSale,
            IsNew = request.IsNew,
            PriceTypeId = pricePolicy.DefaultPriceTypeId,
            PriceTypeRules = pricePolicy.CategoryPriceTypes
                .Select(rule => new CategoryPriceTypeRule(rule.CategoryId, rule.PriceTypeId))
                .ToArray(),
            Page = request.Page,
            PageSize = request.PageSize
        };

        var catalogResult = await sender.Send(new GetProductListQuery(catalogRequest), cancellationToken);

        return catalogResult.Map(MapCatalogPage);
    }

    private static PagedResult<List<SalesCatalogListItemDto>> MapCatalogPage(
        PagedResult<List<ProductListRowDto>> catalogPage)
    {
        var items = catalogPage.Value
            .Select(product => new SalesCatalogListItemDto
            {
                ProductId = product.Id,
                CategoryId = product.CategoryId ?? 0,
                Name = product.Name,
                ProductSlug = product.ProductSlug,
                Photo = product.Photo,
                InStock = product.InStock,
                IsSale = product.IsSale,
                IsNew = product.IsNew,
                Price = product.Price
            })
            .ToList();

        return new PagedResult<List<SalesCatalogListItemDto>>(catalogPage.PagedInfo, items);
    }
}
