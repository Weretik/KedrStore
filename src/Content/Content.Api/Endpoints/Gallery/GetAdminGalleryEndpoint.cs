using Content.Application.Features.Gallery.Queries.GetAdminGallery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Gallery;

internal static class GetAdminGalleryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminGallery")
            .Produces<IReadOnlyList<AdminGalleryItemResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetAdminGalleryQuery(), cancellationToken))
            .Map(GalleryResponseMapper.ToResponse)
            .ToHttpResult(http);
}
