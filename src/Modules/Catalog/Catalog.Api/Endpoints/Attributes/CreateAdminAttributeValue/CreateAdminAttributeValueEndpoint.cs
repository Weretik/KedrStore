using Catalog.Application.Features.Attributes.Commands.CreateAttributeValue;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.CreateAdminAttributeValue;

internal static class CreateAdminAttributeValueEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/values", Handle)
            .WithName("createAdminAttributeValue")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCatalogAttributeDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] CreateAttributeValueRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new CreateAttributeValueCommand(id, request), cancellationToken)).ToHttpResult(http);
}
