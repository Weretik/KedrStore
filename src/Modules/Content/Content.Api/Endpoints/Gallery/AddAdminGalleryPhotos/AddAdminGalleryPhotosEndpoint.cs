using Content.Application.Features.Gallery.Commands.AddGalleryPhotos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Gallery.Shared;

namespace Content.Api.Endpoints.Gallery.AddAdminGalleryPhotos;

internal static class AddAdminGalleryPhotosEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("photos", Handle)
            .WithName("addAdminGalleryPhotos")
            .WithSummary("Add media-library photos to the end of the gallery")
            .WithDescription("Requires `CanManageContent`. Up to 100 file IDs in the order to add; a photo already in the gallery is skipped. A file that is not in the library answers 400 and nothing is added.")
            .Produces<IReadOnlyList<AdminGalleryItemResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    /// <summary>Adds media-library pictures to the end; a picture already in the gallery is skipped.</summary>
    private static async Task<IResult> Handle(
        [FromBody] AddGalleryPhotosRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new AddGalleryPhotosCommand(request.MediaFileIds ?? []), cancellationToken))
            .Map(GalleryResponseMapper.ToResponse)
            .ToHttpResult(http);
}
