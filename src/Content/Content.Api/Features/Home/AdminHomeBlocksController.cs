using Content.Application.Features.Home.Commands.CreateHomeBrand;
using Content.Application.Features.Home.Commands.DeleteHomeBrand;
using Content.Application.Features.Home.Commands.ReorderHomeBrands;
using Content.Application.Features.Home.Commands.UpdateHomeBrand;
using Content.Application.Features.Home.Commands.CreateHomeFaqItem;
using Content.Application.Features.Home.Commands.DeleteHomeFaqItem;
using Content.Application.Features.Home.Commands.ReorderHomeFaq;
using Content.Application.Features.Home.Commands.UpdateHomeFaqItem;
using Content.Application.Features.Home.Queries.GetAdminHomeBlocks;
using Content.Application.Features.Home.Shared;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.Home;

/// <summary>
/// Admin editing of the home page FAQ and brands. Every change answers with both lists in site order, so the page
/// needs no second request.
/// </summary>
[ApiController]
[Route("api/admin/content/home")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminHomeBlocksController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminHomeBlocks")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> Get(CancellationToken cancellationToken)
        => Map(await sender.Send(new GetAdminHomeBlocksQuery(), cancellationToken));

    /// <summary>Adds a question to the end of the FAQ.</summary>
    [HttpPost("faq", Name = "createAdminHomeFaqItem")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> CreateFaq([FromBody] HomeFaqItemRequest request,
        CancellationToken cancellationToken)
        => Map(await sender.Send(new CreateHomeFaqItemCommand(HomeResponseMapper.ToInput(request)), cancellationToken));

    [HttpPut("faq/{id:long}", Name = "updateAdminHomeFaqItem")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> UpdateFaq([FromRoute] long id,
        [FromBody] HomeFaqItemRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new UpdateHomeFaqItemCommand(id, HomeResponseMapper.ToInput(request)),
            cancellationToken));

    [HttpDelete("faq/{id:long}", Name = "deleteAdminHomeFaqItem")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> DeleteFaq([FromRoute] long id,
        CancellationToken cancellationToken)
        => Map(await sender.Send(new DeleteHomeFaqItemCommand(id), cancellationToken));

    /// <summary>Saves the order of all questions. 409 when the list does not match the current FAQ.</summary>
    [HttpPut("faq/order", Name = "reorderAdminHomeFaq")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> ReorderFaq([FromBody] ReorderHomeRequest request,
        CancellationToken cancellationToken)
        => Map(await sender.Send(new ReorderHomeFaqCommand(request.Ids ?? []), cancellationToken));

    /// <summary>Adds a brand to the end; its logo must be a media library file.</summary>
    [HttpPost("brands", Name = "createAdminHomeBrand")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> CreateBrand([FromBody] HomeBrandRequest request,
        CancellationToken cancellationToken)
        => Map(await sender.Send(new CreateHomeBrandCommand(HomeResponseMapper.ToInput(request)), cancellationToken));

    [HttpPut("brands/{id:long}", Name = "updateAdminHomeBrand")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> UpdateBrand([FromRoute] long id,
        [FromBody] HomeBrandRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new UpdateHomeBrandCommand(id, HomeResponseMapper.ToInput(request)),
            cancellationToken));

    /// <summary>Removes a brand; its logo stays in the media library.</summary>
    [HttpDelete("brands/{id:long}", Name = "deleteAdminHomeBrand")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> DeleteBrand([FromRoute] long id,
        CancellationToken cancellationToken)
        => Map(await sender.Send(new DeleteHomeBrandCommand(id), cancellationToken));

    /// <summary>Saves the order of all brands. 409 when the list does not match the current brands.</summary>
    [HttpPut("brands/order", Name = "reorderAdminHomeBrands")]
    [ProducesResponseType(typeof(AdminHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminHomeBlocksResponse>> ReorderBrands([FromBody] ReorderHomeRequest request,
        CancellationToken cancellationToken)
        => Map(await sender.Send(new ReorderHomeBrandsCommand(request.Ids ?? []), cancellationToken));

    private ActionResult<AdminHomeBlocksResponse> Map(Result<AdminHomeBlocks> result)
        => this.ToActionResult(result.Map(HomeResponseMapper.ToResponse));
}
