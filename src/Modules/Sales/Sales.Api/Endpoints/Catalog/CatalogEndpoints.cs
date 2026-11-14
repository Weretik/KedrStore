using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Sales.Api.Endpoints.Catalog;

/// <summary>The manager's catalog with the signed-in customer's prices.</summary>
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var signedIn = endpoints.MapGroup("api/sales/{lang}/catalog")
            .WithTags("Catalog")
            .RequireAuthorization();
        GetCatalogListEndpoint.Map(signedIn);

        return endpoints;
    }
}
