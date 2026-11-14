using Catalog.Application.Features.OneCSync.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Catalog;

/// <summary>«Повне оновлення з 1С»: the whole catalogue in one run, as <c>--job=full</c>.</summary>
internal sealed class FullSyncTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.FullSync;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        await services.GetRequiredService<SyncOneCFullJob>().RunAsync(cancellationToken);
        return "Каталог, залишки й ціни оновлено повністю.";
    }
}
