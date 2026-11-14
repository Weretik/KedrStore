using Media.Application.Features.Files.Commands.UpdateMediaFile;
using Media.Application.Features.Files.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Media.Api.Endpoints.Files;

internal static class UpdateMediaFileEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", Handle)
            .WithName("updateAdminMediaFile")
            .Produces<MediaFileItem>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] UpdateMediaFileRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateMediaFileCommand(id, request.FileName, request.FolderId), cancellationToken))
            .ToHttpResult(http);
}
