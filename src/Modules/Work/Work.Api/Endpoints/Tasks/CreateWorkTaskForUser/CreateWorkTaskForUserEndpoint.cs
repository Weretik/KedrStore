using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Commands.CreateWorkTaskForUser;
using Work.Application.Features.Tasks.Shared;
namespace Work.Api.Endpoints.Tasks.CreateWorkTaskForUser;

internal static class CreateWorkTaskForUserEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("tasks", Handle)
            .WithName("createAdminWorkTask")
            .WithSummary("Put a task on a staff member's board")
            .Produces<WorkTaskDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromBody] CreateWorkTaskForUserRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new CreateWorkTaskForUserCommand(
                request.OwnerUserId, request.Title, request.Details, request.DueDate, request.Steps), cancellationToken))
            .ToHttpResult(http);
}
