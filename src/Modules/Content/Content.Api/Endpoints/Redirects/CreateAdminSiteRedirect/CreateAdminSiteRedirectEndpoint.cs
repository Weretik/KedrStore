using Content.Application.Features.Redirects.Commands.CreateSiteRedirect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Content.Api.Endpoints.Redirects.Shared;

namespace Content.Api.Endpoints.Redirects.CreateAdminSiteRedirect;

internal static class CreateAdminSiteRedirectEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminSiteRedirect")
            .WithSummary("Add a site redirect")
            .WithDescription("Requires `CanManageContent`. The old address is stored as the site matches it (path only, decoded, lower case, no trailing slash). 409 when that address already has a redirect, so repeating the request is safe; 400 for a chain (the new address is redirected itself, or another redirect leads to the old one).")
            .Produces<AdminSiteRedirectResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromBody] SaveSiteRedirectRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateSiteRedirectCommand(request.From, request.To), cancellationToken);
        return result.Map(SiteRedirectResponseMapper.ToResponse).ToHttpResult(http);
    }
}
