using Catalog.Application.Features.Categories.Commands.SetCategorySearchSettings;
using Catalog.Contracts.Category;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Categories.SetAdminCategorySearchSettings;

internal static class SetAdminCategorySearchSettingsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/search-settings", Handle)
            .WithName("setAdminCategorySearchSettings")
            .WithSummary("Close a category from search engines and set its link-preview picture")
            .WithDescription("Spec catalog/011 R-01, R-03. Closing or opening applies to every subcategory too; the site keeps showing a closed category and its products, with noindex and outside the sitemap. A new 1C category takes its parent's mark. The picture is a media-library file; null removes it and the site keeps its own. Repeating the same request is safe.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCategoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetCategorySearchSettingsRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetCategorySearchSettingsCommand(id, request.IsClosedFromSearch, request.ShareImageMediaFileId),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}
