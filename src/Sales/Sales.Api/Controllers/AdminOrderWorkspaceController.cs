using Identity.Application.Security.Policies;
using Sales.Application.Contracts.Orders;

namespace Sales.Api.Controllers;

[ApiController]
[Authorize(Policy = PolicyNames.CanManageOrders)]
[Route("api/admin/order-workspace")]
public sealed class AdminOrderWorkspaceController(IAdminOrderWorkspaceService service) : ControllerBase
{
    [HttpGet("orders", Name = "getAdminWorkspaceOrders")]
    public async Task<ActionResult> GetOrders(
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string sort = "-createdAt", [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try { return Ok(await service.GetOrdersAsync(searchTerm, counterpartyId, sort, page, pageSize, cancellationToken)); }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpGet("orders/{orderId:long}", Name = "getAdminWorkspaceOrderById")]
    public async Task<ActionResult> GetOrder(long orderId, CancellationToken cancellationToken)
    {
        var order = await service.GetOrderAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("quote", Name = "quoteAdminWorkspaceOrder")]
    public async Task<ActionResult> Quote([FromBody] WorkspaceQuoteRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await service.QuoteAsync(request, cancellationToken)); }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpPost("orders", Name = "createAdminWorkspaceOrder")]
    public async Task<ActionResult> CreateOrder(
        [FromBody] WorkspaceCreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.CreateOrderAsync(request, idempotencyKey, cancellationToken);
            if (result.Conflict is not null) return Conflict(result.Conflict);
            return result.Replay ? Ok(result.Order) : StatusCode(201, result.Order);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpGet("drafts", Name = "getAdminOrderDrafts")]
    public async Task<ActionResult> GetDrafts(
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string sort = "-updatedAt", [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try { return Ok(await service.GetDraftsAsync(searchTerm, counterpartyId, sort, page, pageSize, cancellationToken)); }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpGet("drafts/{draftId:long}", Name = "getAdminOrderDraftById")]
    public async Task<ActionResult> GetDraft(long draftId, CancellationToken cancellationToken)
    {
        try
        {
            var draft = await service.GetDraftAsync(draftId, cancellationToken);
            return draft is null ? NotFound() : Ok(draft);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpPost("drafts", Name = "createAdminOrderDraft")]
    public async Task<ActionResult> CreateDraft(
        [FromBody] WorkspaceDraftInput request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.CreateDraftAsync(request, idempotencyKey, cancellationToken);
            return result.Replay ? Ok(result.Draft) : StatusCode(201, result.Draft);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpPut("drafts/{draftId:long}", Name = "updateAdminOrderDraft")]
    public async Task<ActionResult> UpdateDraft(
        long draftId, [FromHeader(Name = "If-Match")] string version,
        [FromBody] WorkspaceDraftInput request, CancellationToken cancellationToken)
    {
        try
        {
            var draft = await service.UpdateDraftAsync(draftId, version, request, cancellationToken);
            return draft is null ? NotFound() : Ok(draft);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpDelete("drafts/{draftId:long}", Name = "deleteAdminOrderDraft")]
    public async Task<ActionResult> DeleteDraft(
        long draftId, [FromHeader(Name = "If-Match")] string version, CancellationToken cancellationToken)
    {
        try { return await service.DeleteDraftAsync(draftId, version, cancellationToken) ? NoContent() : NotFound(); }
        catch (WorkspaceException error) { return Error(error); }
    }

    private ObjectResult Error(WorkspaceException error)
        => StatusCode(error.StatusCode, new { code = error.Code, message = error.Message });
}
