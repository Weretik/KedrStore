using Catalog.Application.Features.Attributes.Queries.GetUnresolvedCodes;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Attributes;

internal static class GetAdminUnresolvedAttributeCodesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("unresolved-codes", Handle)
            .WithName("getAdminUnresolvedAttributeCodes")
            .Produces<List<AdminUnresolvedCodeDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    /// <summary>«Не розшифровані коди».</summary>
    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetUnresolvedCodesQuery(), cancellationToken)).ToHttpResult(http);
}
