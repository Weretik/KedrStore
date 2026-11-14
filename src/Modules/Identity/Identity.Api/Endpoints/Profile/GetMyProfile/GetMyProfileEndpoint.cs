using Identity.Application.Features.Profile.Queries.GetProfile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Identity.Api.Endpoints.Profile.GetMyProfile;

internal static class GetMyProfileEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getMyProfile")
            .Produces<ProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProfileQuery(), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? TypedResults.Ok(ProfileResponse.From(result.Value))
            : result.ToProblem(http);
    }
}
