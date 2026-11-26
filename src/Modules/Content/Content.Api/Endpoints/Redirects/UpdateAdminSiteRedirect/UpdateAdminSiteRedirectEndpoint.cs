using Content.Application.Features.Redirects.Commands.UpdateSiteRedirect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Content.Api.Endpoints.Redirects.Shared;

namespace Content.Api.Endpoints.Redirects.UpdateAdminSiteRedirect;

internal static class UpdateAdminSiteRedirectEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", Handle)
            .WithName("updateAdminSiteRedirect")
            .WithSummary("Change a site redirect")
            .WithDescription("Requires `CanManageContent`. Replaces both addresses under the same rules as the create; a `version` older than the stored one answers 409. Repeating it is safe.")
            .Produces<AdminSiteRedirectResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] SaveSiteRedirectRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateSiteRedirectCommand(id, request.From, request.To, request.Version),
            cancellationToken);
        return result.Map(SiteRedirectResponseMapper.ToResponse).ToHttpResult(http);
    }
}
