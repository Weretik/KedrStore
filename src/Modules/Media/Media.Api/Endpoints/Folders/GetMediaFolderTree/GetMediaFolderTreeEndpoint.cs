using Media.Application.Features.Folders.Queries.GetMediaFolderTree;
using Media.Application.Features.Folders.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Media.Api.Endpoints.Folders.GetMediaFolderTree;

internal static class GetMediaFolderTreeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminMediaFolders")
            .Produces<MediaFolderTree>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetMediaFolderTreeQuery(), cancellationToken)).ToHttpResult(http);
}
