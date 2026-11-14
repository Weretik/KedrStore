using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.Catalog.Queries.GetCatalogList;

namespace Sales.Api.Endpoints.Catalog;

internal static class GetCatalogListEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("products", Handle)
            .Produces<PagedResult<List<CatalogListItemDto>>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ClaimsPrincipal user, ISender sender, HttpContext http,
        CancellationToken cancellationToken,
        [FromBody] CatalogProductsRequest? request = null, [FromQuery(Name = "search")] string? searchTerm = null)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var identityUserId))
            return TypedResults.Unauthorized();

        var catalogRequest = new CatalogRequest
        {
            Lang = lang,
            IdentityUserId = identityUserId,
            SearchTerm = searchTerm,
            CategoryId = request?.CategoryId,
            InStock = request?.InStock,
            IsSale = request?.IsSale,
            IsNew = request?.IsNew,
            Page = request?.Page ?? 1,
            PageSize = request?.PageSize ?? 20
        };

        return (await sender.Send(new GetCatalogListQuery(catalogRequest), cancellationToken)).ToHttpResult(http);
    }
}
