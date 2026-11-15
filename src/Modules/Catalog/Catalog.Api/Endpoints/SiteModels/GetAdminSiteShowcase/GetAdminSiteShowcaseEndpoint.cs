using Catalog.Application.Features.SiteModels.Queries.GetSiteShowcase;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.SiteModels.Shared;

namespace Catalog.Api.Endpoints.SiteModels.GetAdminSiteShowcase;

internal static class GetAdminSiteShowcaseEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("showcase", Handle)
            .WithName("getAdminSiteShowcase")
            .WithSummary("«Вітрина сайту» - models with their on-site products, variant values, and problems")
            .WithDescription("One row per model with on-site products and one row per on-site product in no model (spec catalog/007 R-19). On site means exported from 1C, not deleted, and in a category shown on the site (R-18). Products in no model come first, then other rows with problems, then by name. `categoryId` takes the category with its subcategories; `status` keeps proposed models, accepted models, or products in no model (`none`); `search` matches part of the model or product name, or a 1C code exactly.")
            .Produces<SiteShowcaseListDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [AsParameters] GetSiteModelsParameters parameters, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetSiteShowcaseQuery(parameters.ToShowcaseRequest()), cancellationToken))
            .ToHttpResult(http);
}
