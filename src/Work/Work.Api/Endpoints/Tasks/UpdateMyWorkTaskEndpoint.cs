using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Commands.UpdateMyWorkTask;
using Work.Application.Features.Tasks.Shared;

namespace Work.Api.Endpoints.Tasks;

internal static class UpdateMyWorkTaskEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("tasks/{id:long}", Handle)
            .WithName("updateMyWorkTask")
            .Produces<WorkTaskDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] SaveWorkTaskRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateMyWorkTaskCommand(
            id, request.Title, request.Details, request.DueDate, request.Steps), cancellationToken)).ToHttpResult(http);
}
