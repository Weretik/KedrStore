using Media.Application.Features.Files.Commands.ReplaceMediaFile;
using Media.Application.Features.Files.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Media.Api.Endpoints.Files.Shared;

namespace Media.Api.Endpoints.Files.ReplaceMediaFile;

internal static class ReplaceMediaFileEndpoint
{
    // The host's cookie CSRF check covers form posts, so the Minimal API antiforgery check stays off.
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}/content", Handle)
            .WithName("replaceAdminMediaFile")
            .WithUploadLimit()
            .DisableAntiforgery()
            .Produces<MediaFileItem>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status503ServiceUnavailable);

    private static async Task<IResult> Handle(
        [FromRoute] long id, IFormFile? file, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new ReplaceMediaFileCommand(id, MediaUploads.ToUpload(file)), cancellationToken))
            .ToHttpResult(http);
}
