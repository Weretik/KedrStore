using Catalog.Application.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Site;

/// <summary>«Перебудувати список товарів»: the list of the site and the admin from what is in the database.</summary>
internal sealed class ProductListTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.ProductList;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        await services.GetRequiredService<IProductListProjectionRebuilder>().RebuildAsync(cancellationToken);
        return "Список товарів перебудовано.";
    }
}
