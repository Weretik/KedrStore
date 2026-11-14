using Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Attributes;

internal static class DecideAdminAttributeCodeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("codes/{aliasId:int}", Handle)
            .WithName("decideAdminAttributeCode")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<DecideAttributeCodeResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int aliasId, [FromBody] DecideAttributeCodeRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new DecideAttributeCodeCommand(aliasId, request), cancellationToken)).ToHttpResult(http);
}
