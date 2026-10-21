using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote;

public sealed class QuoteWorkspaceOrderQueryHandler(IWorkspacePricing port)
    : IQueryHandler<QuoteWorkspaceOrderQuery, Result<WorkspaceQuote>>
{
    public async ValueTask<Result<WorkspaceQuote>> Handle(QuoteWorkspaceOrderQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(await port.QuoteAsync(request.Quote, cancellationToken));
    }
}
