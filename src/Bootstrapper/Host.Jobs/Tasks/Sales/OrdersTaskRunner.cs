using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure.Integrations.OneC.Jobs;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Sales;

/// <summary>«Замовлення в 1С»: due order retries, as <c>--job=sync-one-c-orders</c>.</summary>
internal sealed class OrdersTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.Orders;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var result = await services.GetRequiredService<SyncOneCOrdersJob>().RunAsync(cancellationToken);
        if (result.Processed == 0) return "Немає замовлень до надсилання.";
        return $"Замовлень: {result.Processed}; прийнято 1С {result.Accepted}, з помилкою {result.BusinessError + result.TransportError}.";
    }
}
