using Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Requisites.Shared;

namespace Content.Api.Endpoints.Requisites.UpdateAdminCompanyRequisites;

internal static class UpdateAdminCompanyRequisitesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("", Handle)
            .WithName("updateAdminCompanyRequisites")
            .Produces<CompanyRequisitesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromBody] SaveCompanyRequisitesRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCompanyRequisitesCommand(CompanyRequisitesResponseMapper.ToInput(request)), cancellationToken);
        return result.Map(CompanyRequisitesResponseMapper.ToResponse).ToHttpResult(http);
    }
}
