using Content.Application.Features.Requisites.Queries.GetCompanyRequisites;
using Content.Application.Features.Requisites.Queries.GetCompanyRequisitesHistory;
using Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.Requisites;

/// <summary>Admin editing of the company requisites; Admin role only, because a wrong IBAN costs money.</summary>
[ApiController]
[Route("api/admin/content/requisites")]
[Authorize(Policy = PolicyNames.RequireAdminRole)]
public sealed class AdminCompanyRequisitesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminCompanyRequisites")]
    [ProducesResponseType(typeof(CompanyRequisitesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyRequisitesResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyRequisitesQuery(), cancellationToken);
        return this.ToActionResult(result.Map(CompanyRequisitesResponseMapper.ToResponse));
    }

    [HttpPut(Name = "updateAdminCompanyRequisites")]
    [ProducesResponseType(typeof(CompanyRequisitesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyRequisitesResponse>> Update(
        [FromBody] SaveCompanyRequisitesRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCompanyRequisitesCommand(CompanyRequisitesResponseMapper.ToInput(request)), cancellationToken);
        return this.ToActionResult(result.Map(CompanyRequisitesResponseMapper.ToResponse));
    }

    [HttpGet("history", Name = "getAdminCompanyRequisitesHistory")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyRequisitesChangeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyRequisitesChangeResponse>>> History(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyRequisitesHistoryQuery(), cancellationToken);
        return this.ToActionResult(result.Map(CompanyRequisitesResponseMapper.ToResponse));
    }
}
