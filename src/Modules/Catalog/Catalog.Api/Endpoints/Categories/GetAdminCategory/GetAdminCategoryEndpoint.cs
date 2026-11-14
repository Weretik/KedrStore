using System.Security.Claims;
using Catalog.Application.Features.Categories.Queries.GetAdminCategoryById;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.GetAdminCategory;

internal static class GetAdminCategoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", Handle)
            .Produces<AdminCategoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCategoryByIdQuery(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}
