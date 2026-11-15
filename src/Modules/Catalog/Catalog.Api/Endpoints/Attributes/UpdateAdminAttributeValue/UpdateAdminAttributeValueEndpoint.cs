using Catalog.Application.Features.Attributes.Commands.UpdateAttributeValue;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.UpdateAdminAttributeValue;

internal static class UpdateAdminAttributeValueEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/values/{valueId:int}", Handle)
            .WithName("updateAdminAttributeValue")
            .WithSummary("Edit a value (Admin)")
            .WithDescription("Names, order, and swatch (a color or a media-library texture). The dictionary import then leaves it.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCatalogAttributeDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromRoute] int valueId, [FromBody] UpdateAttributeValueRequest request,
        ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new UpdateAttributeValueCommand(id, valueId, request),
            cancellationToken)).ToHttpResult(http);
}
