using Content.Application.Features.Wholesale.Queries.GetPublicWholesalePage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Wholesale.Shared;

namespace Content.Api.Endpoints.Wholesale.GetPublicWholesalePage;

internal static class GetPublicWholesalePageEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/wholesale", Handle)
            .WithName("getPublicWholesalePage")
            .Produces<PublicWholesalePageResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicWholesalePageQuery(lang), cancellationToken);
        return result.Map(WholesaleResponseMapper.ToResponse).ToHttpResult(http);
    }
}
