using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDrafts;
namespace Sales.Api.Endpoints.OrderWorkspace.GetWorkspaceDraftList;

internal static class GetWorkspaceDraftListEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("drafts", Handle)
            .WithName("getAdminOrderDrafts");

    private static async Task<IResult> Handle(
        ISender sender, HttpContext http,
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string? createdFrom, [FromQuery] string? createdTo,
        CancellationToken cancellationToken,
        [FromQuery] string sort = "-updatedAt", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new GetWorkspaceDraftsQuery(searchTerm, counterpartyId,
            createdFrom, createdTo, sort, page, pageSize), cancellationToken);
        return result.ToHttpResult(http);
    }
}
