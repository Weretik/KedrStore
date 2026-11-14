using Media.Application.Features.Folders.Commands.UpdateMediaFolder;
using Media.Application.Features.Folders.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Media.Api.Endpoints.Folders;

internal static class UpdateMediaFolderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", Handle)
            .WithName("updateAdminMediaFolder")
            .Produces<MediaFolderItem>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] SaveMediaFolderRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateMediaFolderCommand(id, request.Name, request.ParentId), cancellationToken))
            .ToHttpResult(http);
}
