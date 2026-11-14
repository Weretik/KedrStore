namespace Sales.Application.Abstractions.Orders;

public interface IWorkspacePricing
{
    Task<DomainResult<WorkspaceQuote>> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken cancellationToken);
    Task<DomainResult<WorkspaceCustomerPrices>> GetCustomerPricesAsync(WorkspaceCustomerPricesRequest request, CancellationToken cancellationToken);
}
