using Catalog.Application.Features.Groups.Queries.GetProductGroups;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Groups.GetAdminProductGroups;

internal static class GetAdminProductGroupsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminProductGroups")
            .Produces<List<AdminProductGroupListItemDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [AsParameters] GetAdminProductGroupsRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetProductGroupsQuery(request), cancellationToken)).ToHttpResult(http);
}
