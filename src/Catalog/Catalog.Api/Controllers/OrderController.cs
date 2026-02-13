using Catalog.Application.Features.Orders.Create;
using Catalog.Application.Features.Orders.Create.DTOs;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOrderCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }
}
