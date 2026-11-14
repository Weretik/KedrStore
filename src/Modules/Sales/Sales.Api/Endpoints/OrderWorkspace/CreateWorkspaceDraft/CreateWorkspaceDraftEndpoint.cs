using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceDraft;
namespace Sales.Api.Endpoints.OrderWorkspace.CreateWorkspaceDraft;

internal static class CreateWorkspaceDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("drafts", Handle)
            .WithName("createAdminOrderDraft");

    // A missing key binds as empty, as it did in the controller; the validator answers 400.
    private static async Task<IResult> Handle(
        [FromBody] WorkspaceDraftInput request, ISender sender, HttpContext http,
        CancellationToken cancellationToken, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey = null)
    {
        var result = await sender.Send(
            new CreateWorkspaceDraftCommand(request, idempotencyKey ?? Guid.Empty), cancellationToken);
        if (!result.IsSuccess) return result.ToProblem(http);

        return result.Value.Replay
            ? TypedResults.Ok(result.Value.Draft)
            : TypedResults.Json(result.Value.Draft, statusCode: StatusCodes.Status201Created);
    }
}
