using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Attributes.Jobs;
using Catalog.Application.Features.OneCSync.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Features.Catalog;

namespace Host.Jobs.Tasks.Catalog;

/// <summary>«Каталог з 1С»: categories and product cards of every root, attributes of new names, the product list.</summary>
internal sealed class CatalogTaskRunner : IScheduledTaskRunner
{
    public string Key => ScheduledTaskKeys.Catalog;

    public async Task<string> RunAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var roots = OneCRoots.Of(services);
        var categories = services.GetRequiredService<SyncOneCCategoryJob>();
        foreach (var root in roots) await categories.RunAsync(root, cancellationToken);
        var products = services.GetRequiredService<SyncOneCProductDetailsJob>();
        foreach (var root in roots) await products.RunAsync(root, cancellationToken, rebuildProjection: false);

        var attributes = await services.GetRequiredService<RecognizeCatalogAttributesJob>()
            .RunAsync(apply: true, productIds: null, cancellationToken);
        await services.GetRequiredService<IProductListProjectionRebuilder>().RebuildAsync(cancellationToken);
        return attributes.Errors.Count == 0
            ? "Категорії й картки товарів оновлено."
            : $"Категорії й картки оновлено; характеристики: помилок {attributes.Errors.Count}, перша: {attributes.Errors[0]}";
    }
}
