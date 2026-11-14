using Catalog.Application.Features.Slugs.Commands.ChangeSlug;
using Catalog.Contracts.Slugs;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Groups.ChangeAdminProductGroupSlug;

internal static class ChangeAdminProductGroupSlugEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/slug", Handle)
            .WithName("changeAdminProductGroupSlug")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<ChangeSlugResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] ChangeSlugRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(
            ChangeSlugCommand.ForProductGroup(id, request), cancellationToken)).ToHttpResult(http);
}
