using Content.Application.Features.Requisites.Queries.GetCompanyRequisites;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Requisites.Shared;

namespace Content.Api.Endpoints.Requisites.GetCompanyRequisites;

internal static class GetCompanyRequisitesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getCompanyRequisites")
            .Produces<CompanyRequisitesResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyRequisitesQuery(), cancellationToken);
        return result.Map(CompanyRequisitesResponseMapper.ToResponse).ToHttpResult(http);
    }
}
