using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Commands.ChangeMyWorkTaskStatus;
using Work.Application.Features.Tasks.Shared;
namespace Work.Api.Endpoints.Tasks.ChangeMyWorkTaskStatus;

internal static class ChangeMyWorkTaskStatusEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("tasks/{id:long}/status", Handle)
            .WithName("changeMyWorkTaskStatus")
            .Produces<WorkTaskDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] ChangeWorkTaskStatusRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new ChangeMyWorkTaskStatusCommand(id, request.Status, request.Result), cancellationToken))
            .ToHttpResult(http);
}
