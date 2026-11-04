using Content.Api.Requisites;
using Content.Application.Features.Requisites.Get;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Public company requisites shown on the site requisites page.</summary>
[ApiController]
[Route("api/content/requisites")]
[AllowAnonymous]
public sealed class CompanyRequisitesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCompanyRequisites")]
    [ProducesResponseType(typeof(CompanyRequisitesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyRequisitesResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyRequisitesQuery(), cancellationToken);
        return this.ToActionResult(result.Map(CompanyRequisitesResponseMapper.ToResponse));
    }
}
