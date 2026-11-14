using Identity.Contracts.Authorization;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Shared;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrders;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrder;
using Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;
using Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceOrder;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDrafts;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOverview;

namespace Sales.Api.Features.OrderWorkspace;

[ApiController]
[Authorize(Policy = PolicyNames.CanManageOrders)]
[Route("api/admin/order-workspace")]
public sealed class AdminOrderWorkspaceController(ISender sender) : ControllerBase
{
    /// <summary>Counts and short lists for the admin «Огляд»; «today» is the current Kyiv day.</summary>
    [HttpGet("overview", Name = "getAdminWorkspaceOverview")]
    [ProducesResponseType(typeof(WorkspaceOverview), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkspaceOverview>> GetOverview(
        [FromServices] TimeProvider clock, CancellationToken cancellationToken)
    {
        var todayFrom = WorkspaceDateRange.TodayStartUtc(clock.GetUtcNow());
        var result = await sender.Send(new GetWorkspaceOverviewQuery(todayFrom), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpGet("orders", Name = "getAdminWorkspaceOrders")]
    public async Task<ActionResult> GetOrders(
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string? createdFrom, [FromQuery] string? createdTo,
        [FromQuery] string sort = "-createdAt", [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetWorkspaceOrdersQuery(searchTerm, counterpartyId,
            createdFrom, createdTo, sort, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpGet("orders/{orderId:long}", Name = "getAdminWorkspaceOrderById")]
    public async Task<ActionResult> GetOrder(long orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceOrderQuery(orderId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPost("quote", Name = "quoteAdminWorkspaceOrder")]
    public async Task<ActionResult> Quote([FromBody] WorkspaceQuoteRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new QuoteWorkspaceOrderQuery(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPost("customer-prices", Name = "getAdminWorkspaceCustomerPrices")]
    public async Task<ActionResult> CustomerPrices([FromBody] WorkspaceCustomerPricesRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceCustomerPricesQuery(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPost("orders", Name = "createAdminWorkspaceOrder")]
    public async Task<ActionResult> CreateOrder(
        [FromBody] WorkspaceCreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateWorkspaceOrderCommand(request, idempotencyKey), cancellationToken);
        if (!result.IsSuccess) return this.ToProblem(result);
        var created = result.Value;
        if (created.Conflict is not null) return this.ToProblem(created.Conflict);
        return created.Replay ? Ok(created.Order) : StatusCode(StatusCodes.Status201Created, created.Order);
    }

    [HttpGet("drafts", Name = "getAdminOrderDrafts")]
    public async Task<ActionResult> GetDrafts(
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string? createdFrom, [FromQuery] string? createdTo,
        [FromQuery] string sort = "-updatedAt", [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetWorkspaceDraftsQuery(searchTerm, counterpartyId,
            createdFrom, createdTo, sort, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpGet("drafts/{draftId:long}", Name = "getAdminOrderDraftById")]
    public async Task<ActionResult> GetDraft(long draftId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceDraftQuery(draftId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpPost("drafts", Name = "createAdminOrderDraft")]
    public async Task<ActionResult> CreateDraft(
        [FromBody] WorkspaceDraftInput request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateWorkspaceDraftCommand(request, idempotencyKey), cancellationToken);
        if (!result.IsSuccess) return this.ToProblem(result);
        return result.Value.Replay ? Ok(result.Value.Draft) : StatusCode(StatusCodes.Status201Created, result.Value.Draft);
    }

    [HttpPut("drafts/{draftId:long}", Name = "updateAdminOrderDraft")]
    public async Task<ActionResult> UpdateDraft(
        long draftId, [FromHeader(Name = "If-Match")] string version,
        [FromBody] WorkspaceDraftInput request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateWorkspaceDraftCommand(draftId, version, request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    [HttpDelete("drafts/{draftId:long}", Name = "deleteAdminOrderDraft")]
    public async Task<ActionResult> DeleteDraft(
        long draftId, [FromHeader(Name = "If-Match")] string version, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWorkspaceDraftCommand(draftId, version), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToProblem(result);
    }
}
