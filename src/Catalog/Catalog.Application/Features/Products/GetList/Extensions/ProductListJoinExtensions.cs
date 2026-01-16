using Catalog.Application.Integrations.OneC.Options;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetList;

public static class ProductListJoinExtensions
{
    public static IQueryable<ProductListRowDto> JoinPricesForList(
        this IQueryable<Product> productsQuery,
        IQueryable<ProductPrice> pricesQuery,
        GetProductsRequest request,
        string hardwareRootCategoryId)
    {
        pricesQuery = pricesQuery.Where(pr => pr.PriceTypeId == PriceTypeId.From(request.PriceTypeId));

        var  productListQuery =
            productsQuery.LeftJoin(pricesQuery,
                p => p.Id,
                pr => pr.ProductId,
                (product, price) => new ProductListRowDto
                {
                    Id = product.Id.Value,
                    Name = product.Name,
                    Photo = product.Photo,
                    ProductSlug = product.ProductSlug,
                    CategoryId = product.CategoryId.Value,
                    InStock = product.ProductTypeIdOneC == hardwareRootCategoryId
                        ? product.Stock > 2
                        : product.Stock > 0,
                    IsSale = product.IsSale,
                    IsNew = product.IsNew,
                    Price = price != null ? price.Amount : null

                });

        if (request.PriceFrom is not null)
            productListQuery = productListQuery
                .Where(x => x.Price != null && x.Price >= request.PriceFrom.Value);

        if (request.PriceTo is not null)
            productListQuery = productListQuery
                .Where(x => x.Price != null && x.Price <= request.PriceTo.Value);


        return productListQuery;
    }

}
