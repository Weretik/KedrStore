using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.UpdateScheduledTaskSchedule;

internal static class UpdateScheduledTaskScheduleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{key}/schedule", Handle)
            .WithName("updateAdminScheduledTaskSchedule")
            .Produces<ScheduledTaskResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(string key, [FromBody] ScheduledTaskScheduleInput input,
        ISender sender, HttpContext http, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateScheduledTaskScheduleCommand(key, input, TasksEndpoints.Actor(http.User)),
            cancellationToken)).ToHttpResult(http);
}
