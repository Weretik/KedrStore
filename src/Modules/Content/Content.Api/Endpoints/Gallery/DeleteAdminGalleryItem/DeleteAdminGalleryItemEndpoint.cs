using Content.Application.Features.Gallery.Commands.DeleteGalleryItem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Gallery.Shared;

namespace Content.Api.Endpoints.Gallery.DeleteAdminGalleryItem;

internal static class DeleteAdminGalleryItemEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("items/{id:long}", Handle)
            .WithName("deleteAdminGalleryItem")
            .Produces<IReadOnlyList<AdminGalleryItemResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Removes a card; the picture stays in the media library.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new DeleteGalleryItemCommand(id), cancellationToken))
            .Map(GalleryResponseMapper.ToResponse)
            .ToHttpResult(http);
}
