using System.Security.Claims;
using Catalog.Application.Features.Categories.Queries.GetCategoryContent;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.GetAdminCategoryContent;

internal static class GetAdminCategoryContentEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}/content", Handle)
            .Produces<AdminCategoryContentResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryContentQuery(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}
