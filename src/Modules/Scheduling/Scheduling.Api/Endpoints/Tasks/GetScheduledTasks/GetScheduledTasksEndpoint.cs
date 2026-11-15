using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.GetScheduledTasks;

internal static class GetScheduledTasksEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminScheduledTasks")
            .WithSummary("List background tasks with their schedules and last results")
            .WithDescription("Every task in page order with its schedule, next run and last success and failure.")
            .Produces<IReadOnlyList<ScheduledTaskResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken) =>
        (await sender.Send(new GetScheduledTasksQuery(), cancellationToken)).ToHttpResult(http);
}
