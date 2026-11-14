using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Media.Api.Endpoints.Folders;

internal static class CreateMediaFolderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminMediaFolder")
            .Produces<MediaFolderItem>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromBody] SaveMediaFolderRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new CreateMediaFolderCommand(request.Name, request.ParentId), cancellationToken))
            .ToHttpResult(http);
}
