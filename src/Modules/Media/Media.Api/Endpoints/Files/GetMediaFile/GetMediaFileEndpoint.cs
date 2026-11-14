using Media.Application.Features.Files.Queries.GetMediaFile;
using Media.Application.Features.Files.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Media.Api.Endpoints.Files.GetMediaFile;

internal static class GetMediaFileEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}", Handle)
            .WithName("getAdminMediaFile")
            .Produces<MediaFileItem>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetMediaFileQuery(id), cancellationToken)).ToHttpResult(http);
}
