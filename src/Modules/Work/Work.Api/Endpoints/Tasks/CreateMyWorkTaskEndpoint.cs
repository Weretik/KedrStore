using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Commands.CreateMyWorkTask;
using Work.Application.Features.Tasks.Shared;

namespace Work.Api.Endpoints.Tasks;

internal static class CreateMyWorkTaskEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("tasks", Handle)
            .WithName("createMyWorkTask")
            .Produces<WorkTaskDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromBody] SaveWorkTaskRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new CreateMyWorkTaskCommand(
            request.Title, request.Details, request.DueDate, request.Steps), cancellationToken)).ToHttpResult(http);
}
