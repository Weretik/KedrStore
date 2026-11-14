using Identity.Application.Features.Profile.Commands.ChangeOwnName;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Profile;

internal static class ChangeMyNameEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("name", Handle)
            .WithName("changeMyName")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] ChangeNameRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeOwnNameCommand(request.FullName), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
