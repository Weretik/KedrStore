using Sales.Application.Abstractions.Workspace.Drafts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDraft;
namespace Sales.Api.Endpoints.OrderWorkspace.GetWorkspaceDraftById;

internal static class GetWorkspaceDraftByIdEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("drafts/{draftId:long}", Handle)
            .WithName("getAdminOrderDraftById")
            .WithSummary("Read one draft with current pricing availability")
            .Produces<WorkspaceDraftDetail>();

    private static async Task<IResult> Handle(
        long draftId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceDraftQuery(draftId), cancellationToken);
        return result.ToHttpResult(http);
    }
}
