using Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.GetScheduledTaskRuns;

internal static class GetScheduledTaskRunsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{key}/runs", Handle)
            .WithName("getAdminScheduledTaskRuns")
            .WithSummary("List recent runs of a background task")
            .WithDescription("The last 50 runs of a task, newest first. Runs are kept 90 days.")
            .Produces<IReadOnlyList<ScheduledTaskRunResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(string key, ISender sender, HttpContext http,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetScheduledTaskRunsQuery(key), cancellationToken)).ToHttpResult(http);
}
