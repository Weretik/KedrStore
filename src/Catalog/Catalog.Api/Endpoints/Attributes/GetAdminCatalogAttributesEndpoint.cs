using Catalog.Application.Features.Attributes.Queries.GetCatalogAttributes;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Attributes;

internal static class GetAdminCatalogAttributesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminCatalogAttributes")
            .Produces<List<AdminCatalogAttributeListItemDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetCatalogAttributesQuery(), cancellationToken)).ToHttpResult(http);
}
