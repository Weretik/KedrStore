using Content.Application.Features.Gallery.Commands.AddGalleryVideo;
using Content.Application.Features.Gallery.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Gallery.Shared;

namespace Content.Api.Endpoints.Gallery.AddAdminGalleryVideo;

internal static class AddAdminGalleryVideoEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("videos", Handle)
            .WithName("addAdminGalleryVideo")
            .Produces<IReadOnlyList<AdminGalleryItemResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Adds a YouTube video by its link to the end. 409 when the video is already there.</summary>
    private static async Task<IResult> Handle(
        [FromBody] AddGalleryVideoRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new AddGalleryVideoCommand(request.Url ?? string.Empty,
            new GalleryTextsInput(request.TitleUk, request.TitleRu, request.CaptionUk, request.CaptionRu)),
            cancellationToken)).Map(GalleryResponseMapper.ToResponse).ToHttpResult(http);
}
