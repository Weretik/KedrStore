using Catalog.Application.Features.Orders.Create;
using Catalog.Application.Features.Orders.Create.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(ISender sender) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(QuickOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuickOrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOrderCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }
}
