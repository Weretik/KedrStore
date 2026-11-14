using Identity.Application.Features.Users.Queries.SearchStaffCandidates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

internal static class SearchStaffCandidatesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("candidates", Handle)
            .WithName("searchStaffCandidates")
            .Produces<IReadOnlyList<StaffCandidateResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    /// <summary>Existing accounts without staff access (usually customers), up to 20, by part of the email or name.</summary>
    private static async Task<IResult> Handle(
        [FromQuery] string? search, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchStaffCandidatesQuery(search ?? string.Empty), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? TypedResults.Ok(result.Value.Select(StaffCandidateResponse.From).ToList())
            : result.ToProblem(http);
    }
}
