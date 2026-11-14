using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.Products.GetAdminProducts;

using Catalog.Api.Endpoints.Products.GetProducts;
using Catalog.Api.Endpoints.Products.GetProductBySlug;
using Catalog.Api.Endpoints.Products.GetAdminProductBySlug;
using Catalog.Api.Endpoints.Products.GetAdminProductGroupOfProduct;
using Catalog.Api.Endpoints.Products.ChangeAdminProductSlug;
using Catalog.Api.Endpoints.Products.SetAdminProductGroupOfProduct;
using Catalog.Api.Endpoints.Products.SetAdminProductMedia;
using Catalog.Api.Endpoints.Products.GetAllAdminProducts;
using Catalog.Api.Endpoints.Products.GetAdminProductMedia;

namespace Catalog.Api.Endpoints.Products;

/// <summary>Products: the admin list, slug, media and group, and the public list and product page by slug.</summary>
public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminProducts = endpoints.MapGroup("api/admin/products")
            .WithTags("AdminProducts")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminProductsEndpoint.Map(adminProducts);
        GetAllAdminProductsEndpoint.Map(adminProducts);
        GetAdminProductBySlugEndpoint.Map(adminProducts);
        ChangeAdminProductSlugEndpoint.Map(adminProducts);
        GetAdminProductMediaEndpoint.Map(adminProducts);
        SetAdminProductMediaEndpoint.Map(adminProducts);
        GetAdminProductGroupOfProductEndpoint.Map(adminProducts);
        SetAdminProductGroupOfProductEndpoint.Map(adminProducts);

        var products = endpoints.MapGroup("api/catalog/{lang}")
            .WithTags("Products")
            .AllowAnonymous();
        GetProductsEndpoint.Map(products);
        GetProductBySlugEndpoint.Map(products);

        return endpoints;
    }
}
