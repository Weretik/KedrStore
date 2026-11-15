using Catalog.Application.Features.ProductMedia.Commands.SetProductMedia;
using Catalog.Contracts.Products.Media;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.SetAdminProductMedia;

internal static class SetAdminProductMediaEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/media", Handle)
            .WithName("setAdminProductMedia")
            .WithSummary("Replace the photos and the scheme of a product")
            .WithDescription("Replaces the whole set with media library files: up to 20 photos in order (the first is the list photo) and an optional scheme. A file can appear once. Every ID must exist in the library.")
            .RequireAuthorization(PolicyNames.CanManageMedia)
            .Produces<ProductMediaDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Replaces the photos (in order) and the scheme of a product with library files (R-31).</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetProductMediaRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetProductMediaCommand(id, request), cancellationToken);
        return result.ToHttpResult(http);
    }
}
