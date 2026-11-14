using Content.Application.Features.Gallery.Queries.PreviewYoutubeVideo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Gallery;

internal static class PreviewAdminGalleryYoutubeVideoEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("youtube-preview", Handle)
            .WithName("previewAdminGalleryYoutubeVideo")
            .Produces<YoutubePreviewResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    /// <summary>Video ID, poster and the title YouTube gives (<c>null</c> if it does not answer) for a pasted link.</summary>
    private static async Task<IResult> Handle(
        [FromQuery] string? url, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PreviewYoutubeVideoQuery(url), cancellationToken);
        return result.Map(GalleryResponseMapper.ToResponse).ToHttpResult(http);
    }
}
