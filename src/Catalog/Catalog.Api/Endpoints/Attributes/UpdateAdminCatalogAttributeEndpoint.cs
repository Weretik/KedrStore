using Catalog.Application.Features.Attributes.Commands.UpdateCatalogAttribute;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Attributes;

internal static class UpdateAdminCatalogAttributeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", Handle)
            .WithName("updateAdminCatalogAttribute")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCatalogAttributeDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] UpdateCatalogAttributeRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateCatalogAttributeCommand(id, request), cancellationToken)).ToHttpResult(http);
}
