using Catalog.Application.Features.Orders.Create;
using Catalog.Application.Features.Orders.Create.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(ISender sender) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("QuickOrder")]
    [ProducesResponseType(typeof(QuickOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<QuickOrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOrderCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }
}
