using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;

public sealed record QuoteWorkspaceOrderQuery(WorkspaceQuoteRequest Quote) : IQuery<Result<WorkspaceQuote>>;
