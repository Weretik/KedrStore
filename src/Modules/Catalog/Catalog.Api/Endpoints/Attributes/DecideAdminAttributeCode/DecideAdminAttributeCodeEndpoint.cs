using Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.DecideAdminAttributeCode;

internal static class DecideAdminAttributeCodeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("codes/{aliasId:int}", Handle)
            .WithName("decideAdminAttributeCode")
            .WithSummary("Decode a code (Admin)")
            .WithDescription("With applyToAll (default true): a value of the code's attribute for all products, or null when the code means nothing to filter by. Product values that waited only for this confirmation become confirmed at once. The dictionary import no longer changes the code. categories, when sent, replaces the code's own decodings per category (spec catalog/008): they win over the decoding for all; a category left out follows it. The products whose names contain the code are recognized again at once.")
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
