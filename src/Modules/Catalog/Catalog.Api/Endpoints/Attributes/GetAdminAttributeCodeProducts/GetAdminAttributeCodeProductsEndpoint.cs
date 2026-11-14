using Catalog.Application.Features.Attributes.Queries.GetCodeProducts;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.GetAdminAttributeCodeProducts;

internal static class GetAdminAttributeCodeProductsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("codes/{aliasId:int}/products", Handle)
            .WithName("getAdminAttributeCodeProducts")
            .Produces<AdminCodeProductsDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>On-site products whose names contain the code, by category, with its own decoding there.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int aliasId, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetCodeProductsQuery(aliasId), cancellationToken)).ToHttpResult(http);
}
