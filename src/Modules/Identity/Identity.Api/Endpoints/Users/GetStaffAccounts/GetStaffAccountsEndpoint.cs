using Identity.Application.Features.Users.Queries.GetStaffList;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Users.Shared;

namespace Identity.Api.Endpoints.Users.GetStaffAccounts;

internal static class GetStaffAccountsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getStaffAccounts")
            .Produces<IReadOnlyList<StaffAccountResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffListQuery(), cancellationToken);
        return TypedResults.Ok(result.Value.Select(StaffAccountResponse.From).ToList());
    }
}
