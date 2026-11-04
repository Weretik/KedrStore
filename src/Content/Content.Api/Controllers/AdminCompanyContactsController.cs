using Content.Api.Contacts;
using Content.Application.Features.Contacts.Get;
using Content.Application.Features.Contacts.Update;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Admin editing of the company contacts shown on the site.</summary>
[ApiController]
[Route("api/admin/content/contacts")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminCompanyContactsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminCompanyContacts")]
    [ProducesResponseType(typeof(AdminCompanyContactsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCompanyContactsResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyContactsQuery(), cancellationToken);
        return this.ToActionResult(result.Map(CompanyContactsResponseMapper.ToAdminResponse));
    }

    [HttpPut(Name = "updateAdminCompanyContacts")]
    [ProducesResponseType(typeof(AdminCompanyContactsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCompanyContactsResponse>> Update(
        [FromBody] SaveCompanyContactsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCompanyContactsCommand(CompanyContactsResponseMapper.ToInput(request)), cancellationToken);
        return this.ToActionResult(result.Map(CompanyContactsResponseMapper.ToAdminResponse));
    }
}
