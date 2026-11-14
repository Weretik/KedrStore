using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.ProductAttributes.AcceptAdminProductReview;

using Catalog.Api.Endpoints.ProductAttributes.ReopenAdminProductReview;
using Catalog.Api.Endpoints.ProductAttributes.RestoreAdminProductAttribute;
using Catalog.Api.Endpoints.ProductAttributes.GetAdminProductAttributes;
using Catalog.Api.Endpoints.ProductAttributes.SetAdminProductAttribute;

namespace Catalog.Api.Endpoints.ProductAttributes;

/// <summary>
/// The «Властивості» panel of a product (spec catalog/006, phase 3): its attribute values and review. Manager and
/// Admin edit them; the original 1C name is never changed.
/// </summary>
public static class ProductAttributesEndpoints
{
    public static IEndpointRouteBuilder MapProductAttributesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminProductAttributes = endpoints.MapGroup("api/admin/products/{productId:int}")
            .WithTags("AdminProductAttributes")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminProductAttributesEndpoint.Map(adminProductAttributes);
        SetAdminProductAttributeEndpoint.Map(adminProductAttributes);
        RestoreAdminProductAttributeEndpoint.Map(adminProductAttributes);
        AcceptAdminProductReviewEndpoint.Map(adminProductAttributes);
        ReopenAdminProductReviewEndpoint.Map(adminProductAttributes);

        return endpoints;
    }
}
