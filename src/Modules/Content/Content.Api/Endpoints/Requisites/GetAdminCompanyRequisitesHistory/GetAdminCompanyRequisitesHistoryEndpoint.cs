using Content.Application.Features.Requisites.Queries.GetCompanyRequisitesHistory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Requisites.Shared;

namespace Content.Api.Endpoints.Requisites.GetAdminCompanyRequisitesHistory;

internal static class GetAdminCompanyRequisitesHistoryEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("history", Handle)
            .WithName("getAdminCompanyRequisitesHistory")
            .Produces<IReadOnlyList<CompanyRequisitesChangeResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyRequisitesHistoryQuery(), cancellationToken);
        return result.Map(CompanyRequisitesResponseMapper.ToResponse).ToHttpResult(http);
    }
}
