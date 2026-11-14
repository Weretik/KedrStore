using Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;
using Catalog.Application.Features.SiteModels.Commands.AddSiteModelProducts;
using Catalog.Application.Features.SiteModels.Commands.CreateSiteModel;
using Catalog.Application.Features.SiteModels.Commands.FillSiteModelFromNames;
using Catalog.Application.Features.SiteModels.Queries.GetSiteModelCategories;
using Catalog.Application.Features.SiteModels.Queries.GetSiteModel;
using Catalog.Application.Features.SiteModels.Queries.GetSiteModelHealth;
using Catalog.Application.Features.SiteModels.Queries.GetSiteModels;
using Catalog.Application.Features.SiteModels.Queries.GetSiteShowcase;
using Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;
using Catalog.Application.Features.SiteModels.Commands.RenameSiteModel;
using Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;
using Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;
using Catalog.Contracts.SiteModels;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Features.SiteModels;

/// <summary>
/// Site models (spec catalog/007): the models the public site is built from, their axes, products, and texts.
/// Manager and Admin edit them; the model slug keeps its spec 006 route, where only Admin and Seo change it.
/// </summary>
[ApiController]
[Route("api/admin/site-models")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminSiteModelsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminSiteModels")]
    [ProducesResponseType(typeof(SiteModelListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteModelListDto>> GetList(
        [FromQuery] GetSiteModelsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSiteModelsQuery(request), cancellationToken));

    [HttpGet("health", Name = "getAdminSiteModelHealth")]
    [ProducesResponseType(typeof(SiteModelHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteModelHealthDto>> GetHealth(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSiteModelHealthQuery(), cancellationToken));

    [HttpGet("showcase", Name = "getAdminSiteShowcase")]
    [ProducesResponseType(typeof(SiteShowcaseListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteShowcaseListDto>> GetShowcase(
        [FromQuery] GetSiteShowcaseRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSiteShowcaseQuery(request), cancellationToken));

    [HttpGet("categories", Name = "getAdminSiteModelCategories")]
    [ProducesResponseType(typeof(List<SiteModelCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SiteModelCategoryDto>>> GetCategories(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSiteModelCategoriesQuery(), cancellationToken));

    [HttpGet("{id:int}", Name = "getAdminSiteModel")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> Get([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSiteModelQuery(id), cancellationToken));

    [HttpPost(Name = "createAdminSiteModel")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteModelDto>> Create(
        [FromBody] CreateSiteModelRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateSiteModelCommand(request), cancellationToken));

    [HttpPut("{id:int}", Name = "renameAdminSiteModel")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> Rename(
        [FromRoute] int id,
        [FromBody] RenameSiteModelRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new RenameSiteModelCommand(id, request), cancellationToken));

    [HttpPost("accept", Name = "acceptAdminSiteModels")]
    [ProducesResponseType(typeof(SiteModelHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SiteModelHealthDto>> Accept(
        [FromBody] AcceptSiteModelsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new AcceptSiteModelsCommand(request), cancellationToken));

    [HttpPut("{id:int}/axes", Name = "setAdminSiteModelAxes")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> SetAxes(
        [FromRoute] int id,
        [FromBody] SetSiteModelAxesRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new SetSiteModelAxesCommand(id, request), cancellationToken));

    [HttpPost("{id:int}/products", Name = "addAdminSiteModelProducts")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> AddProducts(
        [FromRoute] int id,
        [FromBody] AddSiteModelProductsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new AddSiteModelProductsCommand(id, request), cancellationToken));

    [HttpDelete("{id:int}/products/{productId:int}", Name = "removeAdminSiteModelProduct")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> RemoveProduct(
        [FromRoute] int id,
        [FromRoute] int productId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new RemoveSiteModelProductCommand(id, productId), cancellationToken));

    [HttpPut("{id:int}/texts/{language}", Name = "setAdminSiteModelText")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> SetText(
        [FromRoute] int id,
        [FromRoute] string language,
        [FromBody] SetSiteModelTextRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new SetSiteModelTextCommand(id, language, request), cancellationToken));

    [HttpPost("{id:int}/fill-from-names", Name = "fillAdminSiteModelFromNames")]
    [ProducesResponseType(typeof(SiteModelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteModelDto>> FillFromNames([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new FillSiteModelFromNamesCommand(id), cancellationToken));
}
