using Content.Application.Features.Gallery.Commands.ReorderGallery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Gallery;

internal static class ReorderAdminGalleryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("order", Handle)
            .WithName("reorderAdminGallery")
            .Produces<IReadOnlyList<AdminGalleryItemResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Saves the order of all cards. 409 when the list does not match the current gallery.</summary>
    private static async Task<IResult> Handle(
        [FromBody] ReorderGalleryRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new ReorderGalleryCommand(request.Ids ?? []), cancellationToken))
            .Map(GalleryResponseMapper.ToResponse)
            .ToHttpResult(http);
}
