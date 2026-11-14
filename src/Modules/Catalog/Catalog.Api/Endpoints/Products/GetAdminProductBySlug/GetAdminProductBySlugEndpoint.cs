using Catalog.Application.Features.Products.Queries.GetAdminProductBySlug;
using Catalog.Contracts.Products.GetAdminDetail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.GetAdminProductBySlug;

internal static class GetAdminProductBySlugEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("by-slug/{productSlug}", Handle)
            .WithName("getAdminProductBySlug")
            .Produces<AdminProductDetailsDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string productSlug, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminProductBySlugQuery(productSlug), cancellationToken);
        return result.ToHttpResult(http);
    }
}
