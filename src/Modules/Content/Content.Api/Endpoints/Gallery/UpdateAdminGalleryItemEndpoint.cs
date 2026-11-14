using Content.Application.Features.Gallery.Commands.UpdateGalleryItem;
using Content.Application.Features.Gallery.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Gallery;

internal static class UpdateAdminGalleryItemEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("items/{id:long}", Handle)
            .WithName("updateAdminGalleryItem")
            .Produces<IReadOnlyList<AdminGalleryItemResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] UpdateGalleryItemRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateGalleryItemCommand(id,
            new GalleryTextsInput(request.TitleUk, request.TitleRu, request.CaptionUk, request.CaptionRu)),
            cancellationToken)).Map(GalleryResponseMapper.ToResponse).ToHttpResult(http);
}
