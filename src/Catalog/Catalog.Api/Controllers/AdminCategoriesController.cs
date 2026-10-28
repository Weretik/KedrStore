using Catalog.Application.Features.Category.AdminGetById;
using Catalog.Application.Features.Category.AdminGetList;
using Catalog.Contracts.Category;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryResponse>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCategoriesQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AdminCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryResponse>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCategoryByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }
}
