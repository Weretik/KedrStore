using Content.Application.Features.Gallery.Queries.GetPublicGallery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Gallery.Shared;

namespace Content.Api.Endpoints.Gallery.GetPublicGallery;

internal static class GetPublicGalleryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/gallery", Handle)
            .WithName("getPublicGallery")
            .Produces<IReadOnlyList<PublicGalleryItemResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicGalleryQuery(lang), cancellationToken);
        return result.Map(items =>
            (IReadOnlyList<PublicGalleryItemResponse>)items.Select(GalleryResponseMapper.ToResponse).ToList())
            .ToHttpResult(http);
    }
}
