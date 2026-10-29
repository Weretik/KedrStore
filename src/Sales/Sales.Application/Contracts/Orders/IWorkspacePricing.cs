namespace Sales.Application.Contracts.Orders;

public interface IWorkspacePricing
{
    Task<WorkspaceQuote> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken cancellationToken);
    Task<WorkspaceCustomerPrices> GetCustomerPricesAsync(WorkspaceCustomerPricesRequest request, CancellationToken cancellationToken);
}
