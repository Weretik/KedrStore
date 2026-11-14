namespace Sales.Application.Abstractions.Workspace.Pricing;

public interface IWorkspacePricing
{
    Task<DomainResult<WorkspaceQuote>> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken cancellationToken);
    Task<DomainResult<WorkspaceCustomerPrices>> GetCustomerPricesAsync(WorkspaceCustomerPricesRequest request, CancellationToken cancellationToken);
}
