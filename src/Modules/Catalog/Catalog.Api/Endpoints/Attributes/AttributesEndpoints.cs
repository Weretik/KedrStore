using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.Attributes.GetAdminUnresolvedAttributeCodes;

using Catalog.Api.Endpoints.Attributes.SetAdminCategoryAttributeFilters;
using Catalog.Api.Endpoints.Attributes.GetAdminAttributeProblemReasons;
using Catalog.Api.Endpoints.Attributes.UpdateAdminAttributeValue;
using Catalog.Api.Endpoints.Attributes.CreateAdminAttributeValue;
using Catalog.Api.Endpoints.Attributes.GetAdminCatalogAttributes;
using Catalog.Api.Endpoints.Attributes.GetAdminCatalogAttribute;
using Catalog.Api.Endpoints.Attributes.GetAdminCategoryAttributeFilters;
using Catalog.Api.Endpoints.Attributes.CreateAdminCatalogAttribute;
using Catalog.Api.Endpoints.Attributes.GetAdminAttributeCodeProducts;
using Catalog.Api.Endpoints.Attributes.DecideAdminAttributeCode;
using Catalog.Api.Endpoints.Attributes.UpdateAdminCatalogAttribute;
using Catalog.Api.Endpoints.Attributes.UploadAdminAttributeSwatch;

namespace Catalog.Api.Endpoints.Attributes;

/// <summary>
/// The «Властивості» page (spec catalog/006, phase 3): attributes, values, codes, and category filters. Manager,
/// Seo, and Admin read it; only Admin and Seo change it (owner decisions 2026-11-05, 2026-11-06).
/// </summary>
public static class AttributesEndpoints
{
    public static IEndpointRouteBuilder MapAttributesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminCatalogAttributes = endpoints.MapGroup("api/admin/catalog-attributes")
            .WithTags("AdminCatalogAttributes")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminCatalogAttributesEndpoint.Map(adminCatalogAttributes);
        GetAdminCatalogAttributeEndpoint.Map(adminCatalogAttributes);
        CreateAdminCatalogAttributeEndpoint.Map(adminCatalogAttributes);
        UpdateAdminCatalogAttributeEndpoint.Map(adminCatalogAttributes);
        CreateAdminAttributeValueEndpoint.Map(adminCatalogAttributes);
        UpdateAdminAttributeValueEndpoint.Map(adminCatalogAttributes);
        UploadAdminAttributeSwatchEndpoint.Map(adminCatalogAttributes);
        GetAdminUnresolvedAttributeCodesEndpoint.Map(adminCatalogAttributes);
        GetAdminAttributeCodeProductsEndpoint.Map(adminCatalogAttributes);
        DecideAdminAttributeCodeEndpoint.Map(adminCatalogAttributes);
        GetAdminAttributeProblemReasonsEndpoint.Map(adminCatalogAttributes);
        GetAdminCategoryAttributeFiltersEndpoint.Map(adminCatalogAttributes);
        SetAdminCategoryAttributeFiltersEndpoint.Map(adminCatalogAttributes);

        return endpoints;
    }
}
