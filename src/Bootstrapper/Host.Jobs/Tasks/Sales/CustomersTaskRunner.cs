using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.Integrations.OneC.Jobs;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Sales;

/// <summary>«Клієнти з 1С»: counterparties and their price types, as <c>--job=sales-customers-full</c>.</summary>
internal sealed class CustomersTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.Customers;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var result = await services.GetRequiredService<SyncOneCSalesCustomersFullJob>().RunAsync(cancellationToken);
        var clients = result.Counterparties;
        return $"Клієнти: нових {clients.Imported}, змінено {clients.Updated}, видалено {clients.Deleted}.";
    }
}
