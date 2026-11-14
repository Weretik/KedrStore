using Identity.Application.Features.Users.Commands.CreateStaff;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

internal static class CreateStaffAccountEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createStaffAccount")
            .Produces<StaffAccountResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] CreateStaffAccountRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateStaffCommand(request.Email, request.FullName, request.Role, request.Password), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? TypedResults.Json(StaffAccountResponse.From(result.Value), statusCode: StatusCodes.Status201Created)
            : result.ToProblem(http);
    }
}
