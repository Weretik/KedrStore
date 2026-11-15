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
            .WithSummary("Find groups by category and part of the name")
            .WithDescription("Up to 50 groups ordered by name, for the «Перенести в іншу групу» picker.")
            .Produces<List<AdminProductGroupListItemDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [AsParameters] GetAdminProductGroupsRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetProductGroupsQuery(request), cancellationToken)).ToHttpResult(http);
}
