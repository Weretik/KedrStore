using Catalog.Api.Features.Attributes;
using Catalog.Api.Features.Categories;
using Catalog.Api.Features.Groups;
using Catalog.Api.Features.MobileSync;
using Catalog.Api.Features.Overview;
using Catalog.Api.Features.ProductAttributes;
using Catalog.Api.Features.Products;
using Catalog.Api.Features.SiteModels;
using Catalog.Application.Features.Orders.Commands.CreateOrder;
using Catalog.Application.Features.Orders.Commands.CreateOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Catalog.Api.Features.Orders;

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
