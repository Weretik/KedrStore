using Catalog.Application.Features.SiteModels.Commands.ChangeSiteModelCategory;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.ChangeAdminSiteModelCategory;

internal static class ChangeAdminSiteModelCategoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/category", Handle)
            .WithName("changeAdminSiteModelCategory")
            .WithSummary("Move a model to another category")
            .WithDescription("A final category only. All the model's products follow it on the site; each keeps its " +
                             "1C category. A proposed model is accepted in the same step.")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] ChangeSiteModelCategoryRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new ChangeSiteModelCategoryCommand(id, request), cancellationToken)).ToHttpResult(http);
}
