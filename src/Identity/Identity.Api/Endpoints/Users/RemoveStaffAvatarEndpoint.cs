using Identity.Application.Features.Users.Commands.RemoveStaffAvatar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

internal static class RemoveStaffAvatarEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("{userId:guid}/avatar", Handle)
            .WithName("removeStaffAvatar")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(Guid userId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveStaffAvatarCommand(userId), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
