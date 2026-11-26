using Catalog.Api.Endpoints.RetailPrices.GetAdminRetailPriceStatus;
using Catalog.Api.Endpoints.RetailPrices.ImportAdminRetailPrices;
using Catalog.Api.Endpoints.RetailPrices.UnlockAdminRetailPrices;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.RetailPrices;

/// <summary>The site's retail prices from the owner's Excel price file (spec catalog/010).</summary>
public static class RetailPricesEndpoints
{
    public static IEndpointRouteBuilder MapRetailPricesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("api/admin/retail-prices")
            .WithTags("AdminRetailPrices")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminRetailPriceStatusEndpoint.Map(group);
        ImportAdminRetailPricesEndpoint.Map(group);
        UnlockAdminRetailPricesEndpoint.Map(group);

        return endpoints;
    }
}
