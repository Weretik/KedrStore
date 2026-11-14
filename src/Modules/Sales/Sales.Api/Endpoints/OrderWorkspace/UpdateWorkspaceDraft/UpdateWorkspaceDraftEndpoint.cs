using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;
using Sales.Application.Abstractions.Workspace.Drafts;
namespace Sales.Api.Endpoints.OrderWorkspace.UpdateWorkspaceDraft;

internal static class UpdateWorkspaceDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("drafts/{draftId:long}", Handle)
            .WithName("updateAdminOrderDraft");

    // A missing If-Match binds as empty; the validator answers 400.
    private static async Task<IResult> Handle(
        long draftId, [FromBody] WorkspaceDraftInput request, ISender sender, HttpContext http,
        CancellationToken cancellationToken, [FromHeader(Name = "If-Match")] string? version = null)
    {
        var result = await sender.Send(
            new UpdateWorkspaceDraftCommand(draftId, version ?? string.Empty, request), cancellationToken);
        return result.ToHttpResult(http);
    }
}
