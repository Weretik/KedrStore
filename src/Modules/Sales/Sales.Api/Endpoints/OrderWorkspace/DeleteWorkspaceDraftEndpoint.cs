using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;

namespace Sales.Api.Endpoints.OrderWorkspace;

internal static class DeleteWorkspaceDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("drafts/{draftId:long}", Handle)
            .WithName("deleteAdminOrderDraft");

    // A missing If-Match binds as empty; the validator answers 400.
    private static async Task<IResult> Handle(
        long draftId, ISender sender, HttpContext http,
        CancellationToken cancellationToken, [FromHeader(Name = "If-Match")] string? version = null)
    {
        var result = await sender.Send(new DeleteWorkspaceDraftCommand(draftId, version ?? string.Empty), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
