using Content.Api.Contacts;
using Content.Application.Features.Contacts.Get;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Public company contacts that every site page shows.</summary>
[ApiController]
[Route("api/content/contacts")]
[AllowAnonymous]
public sealed class CompanyContactsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCompanyContacts")]
    [ProducesResponseType(typeof(CompanyContactsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyContactsResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyContactsQuery(), cancellationToken);
        return this.ToActionResult(result.Map(CompanyContactsResponseMapper.ToResponse));
    }
}
