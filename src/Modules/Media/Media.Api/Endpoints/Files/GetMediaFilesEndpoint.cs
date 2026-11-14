using Media.Application.Features.Files.Queries.GetMediaFiles;
using Media.Application.Features.Files.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Media.Api.Endpoints.Files;

internal static class GetMediaFilesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminMediaFiles")
            .Produces<MediaFilePage>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        ISender sender, HttpContext http, CancellationToken cancellationToken,
        [FromQuery] long? folderId = null, [FromQuery] bool inRoot = false, [FromQuery] string? search = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 60, [FromQuery] bool includeSubfolders = false)
        => (await sender.Send(new GetMediaFilesQuery(folderId, inRoot, search, page, pageSize, includeSubfolders),
            cancellationToken)).ToHttpResult(http);
}
