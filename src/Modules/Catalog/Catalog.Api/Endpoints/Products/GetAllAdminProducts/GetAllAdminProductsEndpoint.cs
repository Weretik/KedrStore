using Catalog.Application.Features.Products.Queries.GetAllAdminProducts;
using Catalog.Contracts.Products.GetAdminList;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.GetAllAdminProducts;

internal static class GetAllAdminProductsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("all", Handle)
            .Produces<List<AdminProductListRowDto>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllAdminProductsQuery(), cancellationToken);

        return result.ToHttpResult(http);
    }
}
