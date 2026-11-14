using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Queries.GetMyWorkTasks;
using Work.Application.Features.Tasks.Shared;
namespace Work.Api.Endpoints.Tasks.GetMyWorkTasks;

internal static class GetMyWorkTasksEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("tasks", Handle)
            .WithName("getMyWorkTasks")
            .Produces<IReadOnlyList<WorkTaskDto>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetMyWorkTasksQuery(), cancellationToken)).ToHttpResult(http);
}
