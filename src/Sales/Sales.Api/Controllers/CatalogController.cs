namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/{lang}/catalog")]
[Authorize]
public sealed class CatalogController(ISender sender) : ControllerBase
{
    [HttpPost("products")]
    [ProducesResponseType(typeof(PagedResult<List<CatalogListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<List<CatalogListItemDto>>>> Get(
        [FromBody] CatalogProductsRequest? request,
        [FromRoute] string lang,
        [FromQuery(Name = "search")] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var identityUserId))
        {
            return Unauthorized();
        }

        var catalogRequest = new CatalogRequest
        {
            Lang = lang,
            IdentityUserId = identityUserId,
            SearchTerm = searchTerm,
            CategoryId = request?.CategoryId,
            InStock = request?.InStock,
            IsSale = request?.IsSale,
            IsNew = request?.IsNew,
            Page = request?.Page ?? 1,
            PageSize = request?.PageSize ?? 20
        };

        var result = await sender.Send(new GetCatalogListQuery(catalogRequest), cancellationToken);
        return this.ToActionResult(result);
    }
}
