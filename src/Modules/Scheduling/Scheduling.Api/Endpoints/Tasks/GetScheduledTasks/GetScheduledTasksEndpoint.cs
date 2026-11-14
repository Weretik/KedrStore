using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.GetScheduledTasks;

internal static class GetScheduledTasksEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminScheduledTasks")
            .Produces<IReadOnlyList<ScheduledTaskResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken) =>
        (await sender.Send(new GetScheduledTasksQuery(), cancellationToken)).ToHttpResult(http);
}
