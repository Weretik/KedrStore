using Media.Application.Features.Files.Commands.MoveMediaFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Media.Api.Endpoints.Files.MoveMediaFiles;

internal static class MoveMediaFilesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("move", Handle)
            .WithName("moveAdminMediaFiles")
            .Produces<MoveMediaFilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromBody] MoveMediaFilesRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MoveMediaFilesCommand(request.FileIds ?? [], request.FolderId),
            cancellationToken);
        return result.Map(moved => new MoveMediaFilesResponse(moved)).ToHttpResult(http);
    }
}
