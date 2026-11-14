using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Commands.SetMyWorkTaskStep;
using Work.Application.Features.Tasks.Shared;

namespace Work.Api.Endpoints.Tasks;

internal static class SetMyWorkTaskStepEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("tasks/{id:long}/steps/{index:int}", Handle)
            .WithName("setMyWorkTaskStep")
            .Produces<WorkTaskDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromRoute] int index, [FromBody] SetWorkTaskStepRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new SetMyWorkTaskStepCommand(id, index, request.IsDone), cancellationToken))
            .ToHttpResult(http);
}
