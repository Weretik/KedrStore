using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Media.Api.Endpoints.Files;

internal static class UploadMediaFileEndpoint
{
    // The host's cookie CSRF check covers form posts, so the Minimal API antiforgery check stays off.
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("uploadAdminMediaFile")
            .WithUploadLimit()
            .DisableAntiforgery()
            .Produces<MediaFileItem>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status503ServiceUnavailable);

    private static async Task<IResult> Handle(
        IFormFile? file, [FromForm] long? folderId, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UploadMediaFileCommand(folderId, MediaUploads.ToUpload(file)), cancellationToken))
            .ToHttpResult(http);
}
