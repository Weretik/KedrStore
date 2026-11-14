using Catalog.Application.Features.Attributes.Queries.GetCatalogAttribute;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.GetAdminCatalogAttribute;

internal static class GetAdminCatalogAttributeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", Handle)
            .WithName("getAdminCatalogAttribute")
            .Produces<AdminCatalogAttributeDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetCatalogAttributeQuery(id), cancellationToken)).ToHttpResult(http);
}
