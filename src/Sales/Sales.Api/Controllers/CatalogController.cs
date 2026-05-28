namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/{lang}/catalog")]
[AllowAnonymous]
public sealed class CatalogController(ISender sender) : ControllerBase
{
    [HttpGet("products")]
    [ProducesResponseType(typeof(PagedResult<List<CatalogListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<List<CatalogListItemDto>>>> Get(
        [FromQuery] CatalogRequest request,
        [FromRoute] string lang,
        CancellationToken cancellationToken)
    {
        request = request with { Lang = lang };

        var result = await sender.Send(new GetCatalogListQuery(request), cancellationToken);
        return this.ToActionResult(result);
    }
}
