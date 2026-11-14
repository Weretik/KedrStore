using Catalog.Application.Features.Products.Queries.GetAdminProductList;
using Catalog.Contracts.Products.GetAdminList;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.GetAdminProducts;

internal static class GetAdminProductsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .Produces<PagedResult<List<AdminProductListRowDto>>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [AsParameters] GetAdminProductsParameters parameters, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var request = parameters.ToRequest();
        if (request.Deleted == true && !http.User.IsInRole(RoleNames.Admin) && !http.User.IsInRole(RoleNames.Seo))
            return TypedResults.Forbid();

        var result = await sender.Send(new GetAdminProductListQuery(request), cancellationToken);

        return result.ToHttpResult(http);
    }
}
