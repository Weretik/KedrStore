using Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.RunScheduledTaskNow;

internal static class RunScheduledTaskNowEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{key}/run", Handle)
            .WithName("runAdminScheduledTaskNow")
            .Produces<ScheduledTaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(string key, ISender sender, HttpContext http,
        CancellationToken cancellationToken) =>
        (await sender.Send(new RunScheduledTaskNowCommand(key, TasksEndpoints.Actor(http.User)), cancellationToken))
        .ToHttpResult(http);
}
