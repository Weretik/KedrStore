using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Pricing;

namespace Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;

public sealed record QuoteWorkspaceOrderQuery(WorkspaceQuoteRequest Quote) : IQuery<Result<WorkspaceQuote>>;
