using Catalog.Application.Features.Category;
using Catalog.Application.Features.Category.GetBySlug;
using Catalog.Application.Features.Category.GetList;
using Catalog.Application.Features.Category.GetList.DTOs;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/{lang}")]
[AllowAnonymous]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> Get(
        [FromRoute] string lang,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoriesQuery(new CategoryFilter(), lang), cancellationToken);
        if (!result.IsSuccess) return BadRequest();
        return Ok(result.Value.Select(CategoryReadMapper.ToResponse).ToList());
    }

    [HttpGet("categories/by-slug/{categorySlug}")]
    [ProducesResponseType(typeof(CategoryDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDetailsResponse>> GetBySlug(
        [FromRoute] string lang,
        [FromRoute] string categorySlug,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryBySlugQuery(categorySlug, lang), cancellationToken);
        return this.ToActionResult(result);
    }
}
