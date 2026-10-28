using Identity.Application.Security.Policies;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.GetList;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.GetById;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.Create;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetList;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.GetById;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.Create;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.Update;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.Delete;

namespace Sales.Api.Controllers;

[ApiController]
[Authorize(Policy = PolicyNames.CanManageOrders)]
[Route("api/admin/order-workspace")]
public sealed class AdminOrderWorkspaceController(ISender sender) : ControllerBase
{
    [HttpGet("orders", Name = "getAdminWorkspaceOrders")]
    public async Task<ActionResult> GetOrders(
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string? createdFrom, [FromQuery] string? createdTo,
        [FromQuery] string sort = "-createdAt", [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var dates = WorkspaceDateRange.Parse(createdFrom, createdTo);
            return Ok((await sender.Send(new GetWorkspaceOrdersQuery(searchTerm, counterpartyId,
                dates.FromUtc, dates.ToUtcExclusive, sort, page, pageSize), cancellationToken)).Value);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpGet("orders/{orderId:long}", Name = "getAdminWorkspaceOrderById")]
    public async Task<ActionResult> GetOrder(long orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceOrderQuery(orderId), cancellationToken);
        return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result.Value);
    }

    [HttpPost("quote", Name = "quoteAdminWorkspaceOrder")]
    public async Task<ActionResult> Quote([FromBody] WorkspaceQuoteRequest request, CancellationToken cancellationToken)
    {
        try { return Ok((await sender.Send(new QuoteWorkspaceOrderQuery(request), cancellationToken)).Value); }
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
            var result = (await sender.Send(new CreateWorkspaceOrderCommand(request, idempotencyKey), cancellationToken)).Value;
            if (result.Conflict is not null) return Conflict(result.Conflict);
            return result.Replay ? Ok(result.Order) : StatusCode(201, result.Order);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpGet("drafts", Name = "getAdminOrderDrafts")]
    public async Task<ActionResult> GetDrafts(
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string? createdFrom, [FromQuery] string? createdTo,
        [FromQuery] string sort = "-updatedAt", [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var dates = WorkspaceDateRange.Parse(createdFrom, createdTo);
            return Ok((await sender.Send(new GetWorkspaceDraftsQuery(searchTerm, counterpartyId,
                dates.FromUtc, dates.ToUtcExclusive, sort, page, pageSize), cancellationToken)).Value);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpGet("drafts/{draftId:long}", Name = "getAdminOrderDraftById")]
    public async Task<ActionResult> GetDraft(long draftId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new GetWorkspaceDraftQuery(draftId), cancellationToken);
            return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result.Value);
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
            var result = (await sender.Send(new CreateWorkspaceDraftCommand(request, idempotencyKey), cancellationToken)).Value;
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
            var result = await sender.Send(new UpdateWorkspaceDraftCommand(draftId, version, request), cancellationToken);
            return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result.Value);
        }
        catch (WorkspaceException error) { return Error(error); }
    }

    [HttpDelete("drafts/{draftId:long}", Name = "deleteAdminOrderDraft")]
    public async Task<ActionResult> DeleteDraft(
        long draftId, [FromHeader(Name = "If-Match")] string version, CancellationToken cancellationToken)
    {
        try { return (await sender.Send(new DeleteWorkspaceDraftCommand(draftId, version), cancellationToken)).Value ? NoContent() : NotFound(); }
        catch (WorkspaceException error) { return Error(error); }
    }

    private ObjectResult Error(WorkspaceException error)
        => StatusCode(error.StatusCode, new { code = error.Code, message = error.Message });
}
