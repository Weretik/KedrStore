using Content.Application.Features.SiteSeo.Queries.ListAdminPageSeo;
using Content.Application.Features.SiteSeo.Commands.UpdatePageSeo;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.SiteSeo;

/// <summary>Admin editing of the search titles and descriptions of the site pages.</summary>
[ApiController]
[Route("api/admin/content/page-seo")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminPageSeoController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "listAdminPageSeo")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminPageSeoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminPageSeoResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListAdminPageSeoQuery(), cancellationToken);
        return this.ToActionResult(result.Map(PageSeoResponseMapper.ToResponse));
    }

    [HttpPut("{page}", Name = "updateAdminPageSeo")]
    [ProducesResponseType(typeof(AdminPageSeoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminPageSeoResponse>> Update([FromRoute] string page,
        [FromBody] SavePageSeoRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePageSeoCommand(page, request.TitleUk, request.TitleRu,
            request.DescriptionUk, request.DescriptionRu), cancellationToken);
        return this.ToActionResult(result.Map(PageSeoResponseMapper.ToResponse));
    }
}
