using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Products;

public sealed class ProductPhotoCheckStore(CatalogDbContext dbContext) : IProductPhotoCheckStore
{
    public async Task<IReadOnlyList<ProductPhotoCandidate>> GetActiveProductBatchAsync(
        int afterProductId,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .FromSqlInterpolated($"SELECT * FROM \"Products\" WHERE \"Id\" > {afterProductId}")
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Take(take)
            .Select(product => new ProductPhotoCandidate(product.Id, product.Photo))
            .ToListAsync(cancellationToken);
    }

    public async Task ReconcileAsync(
        IReadOnlyCollection<ProductPhotoCandidate> products,
        CancellationToken cancellationToken)
    {
        if (products.Count == 0)
            return;

        var productIds = products.Select(product => product.ProductId).ToArray();
        var existing = await dbContext.ProductPhotoChecks
            .Where(check => productIds.Contains(check.Id))
            .ToDictionaryAsync(check => check.Id, cancellationToken);

        foreach (var product in products)
        {
            if (existing.TryGetValue(product.ProductId, out var check))
            {
                check.ResetForUrl(product.PhotoUrl);
            }
            else
            {
                dbContext.ProductPhotoChecks.Add(
                    ProductPhotoCheck.CreateUnknown(product.ProductId, product.PhotoUrl));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveResultsAsync(
        IReadOnlyCollection<ProductPhotoCheckUpdate> updates,
        DateTimeOffset checkedAtUtc,
        CancellationToken cancellationToken)
    {
        if (updates.Count == 0)
            return 0;

        var productIds = updates.Select(update => update.ProductId).ToArray();
        var checks = await dbContext.ProductPhotoChecks
            .Where(check => productIds.Contains(check.Id))
            .ToDictionaryAsync(check => check.Id, cancellationToken);
        var staleCount = 0;

        foreach (var update in updates)
        {
            if (!checks.TryGetValue(update.ProductId, out var check) ||
                !string.Equals(check.PhotoUrl, update.PhotoUrl, StringComparison.Ordinal))
            {
                staleCount++;
                continue;
            }

            Apply(check, update, checkedAtUtc);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            staleCount += exception.Entries.Count;
            foreach (var entry in exception.Entries)
                entry.State = EntityState.Detached;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        dbContext.ChangeTracker.Clear();
        return staleCount;
    }

    private static void Apply(
        ProductPhotoCheck check,
        ProductPhotoCheckUpdate update,
        DateTimeOffset checkedAtUtc)
    {
        var result = update.Result;
        switch (result.Status)
        {
            case ProductPhotoStatus.Available:
                check.RecordAvailable(update.PhotoUrl, checkedAtUtc, result.HttpStatusCode!.Value);
                break;
            case ProductPhotoStatus.Missing:
                check.RecordMissing(update.PhotoUrl, checkedAtUtc, result.HttpStatusCode!.Value);
                break;
            case ProductPhotoStatus.InvalidContentType:
                check.RecordInvalidContentType(
                    update.PhotoUrl,
                    checkedAtUtc,
                    result.HttpStatusCode!.Value,
                    result.DiagnosticCode!,
                    result.DiagnosticMessage);
                break;
            case ProductPhotoStatus.CheckFailed:
                check.RecordCheckFailed(
                    update.PhotoUrl,
                    checkedAtUtc,
                    result.HttpStatusCode,
                    result.DiagnosticCode!,
                    result.DiagnosticMessage);
                break;
            case ProductPhotoStatus.Unknown:
            default:
                throw new InvalidOperationException("A photo probe must return a terminal status.");
        }
    }
}
