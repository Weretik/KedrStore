using Sales.Api.Contracts.Orders;
using Sales.Application.Features.Orders.Create;
using Sales.Application.Features.Orders.Create.DTOs;
using Sales.Application.Features.Orders.GetSyncStatus;
using Sales.Application.Features.Orders.GetSyncStatus.DTOs;
using Sales.Application.Features.Orders.RetrySync;
using Sales.Application.Features.Orders.RetrySync.DTOs;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[AllowAnonymous]
public sealed class AdminOrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateAdminOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CreateAdminOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(
        [FromBody] CreateAdminOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            new CreateOrderRequest(
                request.CounterpartyId,
                request.Comment,
                request.Lines.Select(line => new CreateOrderLineRequest(line.ProductId, line.Quantity, line.Amount)).ToArray()),
            idempotencyKey);
        var result = await sender.Send(command, cancellationToken);

        return result.Status switch
        {
            ResultStatus.Ok => Ok(ToResponse(result.Value)),
            ResultStatus.Created => StatusCode(StatusCodes.Status201Created, ToResponse(result.Value)),
            _ => this.ToActionResult(result).Result!
        };
    }

    [HttpGet("{orderId:long}/sync-status")]
    [ProducesResponseType(typeof(GetAdminOrderSyncStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetSyncStatus(long orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderSyncStatusQuery(orderId), cancellationToken);
        return result.Status == ResultStatus.Ok
            ? Ok(ToResponse(result.Value))
            : this.ToActionResult(result).Result!;
    }

    [HttpPost("{orderId:long}/sync/retry")]
    [ProducesResponseType(typeof(RetryAdminOrderSyncResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> RetrySync(
        long orderId,
        [FromBody] RetryAdminOrderSyncRequest request,
        CancellationToken cancellationToken)
    {
        var requestedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.Identity?.Name
                          ?? "anonymous-development";
        var result = await sender.Send(
            new RetryOrderSyncCommand(orderId, request.Reason, requestedBy),
            cancellationToken);

        return result.Status == ResultStatus.Ok
            ? StatusCode(StatusCodes.Status202Accepted, ToResponse(result.Value))
            : this.ToActionResult(result).Result!;
    }

    private static CreateAdminOrderResponse ToResponse(CreateOrderResult result)
        => new(result.OrderId, result.OrderNumber, result.SyncStatus.ToString());

    private static GetAdminOrderSyncStatusResponse ToResponse(GetOrderSyncStatusResult result)
        => new(
            result.OrderId,
            result.OrderNumber,
            result.SyncStatus.ToString(),
            result.OneCDocumentNumber,
            result.AcceptedAtUtc);

    private static RetryAdminOrderSyncResponse ToResponse(RetryOrderSyncResult result)
        => new(result.OrderId, result.OrderNumber, result.SyncStatus.ToString());
}
