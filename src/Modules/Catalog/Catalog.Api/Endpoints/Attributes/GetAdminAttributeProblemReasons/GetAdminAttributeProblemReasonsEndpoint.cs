using Catalog.Application.Features.Attributes.Queries.GetProblemReasons;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.GetAdminAttributeProblemReasons;

internal static class GetAdminAttributeProblemReasonsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("problem-reasons", Handle)
            .WithName("getAdminAttributeProblemReasons")
            .Produces<List<AdminProblemReasonDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    /// <summary>Reasons of on-site products in review, for the «За причиною» filter of the product list.</summary>
    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetProblemReasonsQuery(), cancellationToken)).ToHttpResult(http);
}
