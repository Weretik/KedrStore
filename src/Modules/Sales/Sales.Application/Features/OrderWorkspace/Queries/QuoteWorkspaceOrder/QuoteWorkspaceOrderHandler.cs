using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;

public sealed class QuoteWorkspaceOrderHandler(IWorkspacePricing port)
    : IQueryHandler<QuoteWorkspaceOrderQuery, Result<WorkspaceQuote>>
{
    public async ValueTask<Result<WorkspaceQuote>> Handle(QuoteWorkspaceOrderQuery request, CancellationToken cancellationToken)
        => (await port.QuoteAsync(request.Quote, cancellationToken)).AsResult();
}
