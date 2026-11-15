using Catalog.Application.Features.SiteModels.Queries.GetSiteModelCategories;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.GetAdminSiteModelCategories;

internal static class GetAdminSiteModelCategoriesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("categories", Handle)
            .WithName("getAdminSiteModelCategories")
            .WithSummary("Category tree with models and problems, for «Моделі» and «Вітрина сайту»")
            .WithDescription("Categories shown on the site (R-18) with on-site products in them or below them, each with its parent; counts include subcategories. Ordered by name; the client builds the tree from `parentId`.")
            .Produces<List<SiteModelCategoryDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetSiteModelCategoriesQuery(), cancellationToken)).ToHttpResult(http);
}
