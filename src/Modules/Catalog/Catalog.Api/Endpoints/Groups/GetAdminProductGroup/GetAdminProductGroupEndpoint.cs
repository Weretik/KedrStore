using Catalog.Application.Features.Groups.Queries.GetProductGroup;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Groups.GetAdminProductGroup;

internal static class GetAdminProductGroupEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", Handle)
            .WithName("getAdminProductGroup")
            .Produces<AdminProductGroupDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetProductGroupQuery(id), cancellationToken)).ToHttpResult(http);
}
