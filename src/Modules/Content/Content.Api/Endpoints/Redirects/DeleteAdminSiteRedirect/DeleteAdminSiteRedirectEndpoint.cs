using Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Redirects.DeleteAdminSiteRedirect;

internal static class DeleteAdminSiteRedirectEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", Handle)
            .WithName("deleteAdminSiteRedirect")
            .WithSummary("Remove a site redirect")
            .WithDescription("Requires `CanManageContent`. A repeat answers 404.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSiteRedirectCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToHttpResult(http);
    }
}
