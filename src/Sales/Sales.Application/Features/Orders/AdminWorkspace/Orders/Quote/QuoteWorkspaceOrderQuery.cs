using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote;

public sealed record QuoteWorkspaceOrderQuery(WorkspaceQuoteRequest Quote) : IQuery<Result<WorkspaceQuote>>;
