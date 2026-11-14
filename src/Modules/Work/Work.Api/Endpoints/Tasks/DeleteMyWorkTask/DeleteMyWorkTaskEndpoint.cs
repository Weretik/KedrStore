using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Work.Application.Features.Tasks.Commands.DeleteMyWorkTask;
namespace Work.Api.Endpoints.Tasks.DeleteMyWorkTask;

internal static class DeleteMyWorkTaskEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("tasks/{id:long}", Handle)
            .WithName("deleteMyWorkTask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMyWorkTaskCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
