using Catalog.Application.Features.ProductAttributes.AcceptReview;
using Catalog.Application.Features.ProductAttributes.Get;
using Catalog.Application.Features.ProductAttributes.ReopenReview;
using Catalog.Application.Features.ProductAttributes.Restore;
using Catalog.Application.Features.ProductAttributes.Set;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

/// <summary>
/// The «Властивості» panel of a product (spec catalog/006, phase 3): its attribute values and review. Manager and
/// Admin edit them; the original 1C name is never changed.
/// </summary>
[ApiController]
[Route("api/admin/products/{productId:int}")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminProductAttributesController(ISender sender) : ControllerBase
{
    [HttpGet("attributes", Name = "getAdminProductAttributes")]
    [ProducesResponseType(typeof(AdminProductAttributesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductAttributesDto>> Get([FromRoute] int productId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductAttributesQuery(productId), cancellationToken));

    /// <summary>Sets the values of one attribute by hand; they become manual and locked.</summary>
    [HttpPut("attributes/{attributeId:int}", Name = "setAdminProductAttribute")]
    [ProducesResponseType(typeof(AdminProductAttributesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductAttributesDto>> Set([FromRoute] int productId,
        [FromRoute] int attributeId, [FromBody] SetProductAttributeRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new SetProductAttributeCommand(productId, attributeId, request),
            cancellationToken));

    /// <summary>«Повернути автоматичне»: drops the manual values of one attribute.</summary>
    [HttpDelete("attributes/{attributeId:int}/manual", Name = "restoreAdminProductAttribute")]
    [ProducesResponseType(typeof(AdminProductAttributesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductAttributesDto>> Restore([FromRoute] int productId,
        [FromRoute] int attributeId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new RestoreProductAttributeCommand(productId, attributeId),
            cancellationToken));

    /// <summary>«Прийняти як є»: the product stops counting as a problem until a person reopens it.</summary>
    [HttpPost("review/accept", Name = "acceptAdminProductReview")]
    [ProducesResponseType(typeof(AdminProductAttributesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductAttributesDto>> Accept([FromRoute] int productId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new AcceptProductReviewCommand(productId), cancellationToken));

    /// <summary>«Повернути на перевірку».</summary>
    [HttpPost("review/reopen", Name = "reopenAdminProductReview")]
    [ProducesResponseType(typeof(AdminProductAttributesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductAttributesDto>> Reopen([FromRoute] int productId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ReopenProductReviewCommand(productId), cancellationToken));
}
