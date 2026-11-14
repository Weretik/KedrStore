using Content.Application.Features.Wholesale.Queries.GetAdminWholesalePage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Wholesale.Shared;

namespace Content.Api.Endpoints.Wholesale.GetAdminWholesalePage;

internal static class GetAdminWholesalePageEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminWholesalePage")
            .Produces<AdminWholesalePageResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminWholesalePageQuery(), cancellationToken);
        return result.Map(WholesaleResponseMapper.ToResponse).ToHttpResult(http);
    }
}
