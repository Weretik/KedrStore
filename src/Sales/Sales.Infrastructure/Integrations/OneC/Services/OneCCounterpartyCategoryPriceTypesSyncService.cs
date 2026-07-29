using Catalog.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Customers.Entities;

namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed class OneCCounterpartyCategoryPriceTypesSyncService(
    ISalesOneCReadClient oneCClient,
    SalesDbContext salesDbContext,
    ICatalogReferenceDataReader catalogReferenceDataReader,
    ILogger<OneCCounterpartyCategoryPriceTypesSyncService> logger)
{
    public async Task<CounterpartyCategoryPriceTypesSyncResult> RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCCounterpartyCategoryPriceTypesJob started");

        var rules = await oneCClient.GetCounterpartyCategoryPriceTypesAsync(cancellationToken);
        if (rules.Count == 0)
        {
            logger.LogWarning("No counterparty category price rules received from 1C. Sync stopping.");
            return new CounterpartyCategoryPriceTypesSyncResult(0, 0, 0);
        }

        var deduped = rules
            .Where(item => !string.IsNullOrWhiteSpace(item.CounterpartyId))
            .GroupBy(item => (CounterpartyId: item.CounterpartyId.Trim(), item.CategoryId), CounterpartyCategoryKeyComparer.Instance)
            .Select(group => group.Last() with { CounterpartyId = group.Key.CounterpartyId })
            .ToArray();

        var counterparties = await salesDbContext.Counterparties
            .AsNoTracking()
            .Select(item => item.Id)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

        var requestedCategoryIds = deduped
            .Where(item => item.CategoryId > 0)
            .Select(item => item.CategoryId)
            .Distinct()
            .ToArray();

        var requestedPriceTypeIds = deduped
            .Where(item => item.PriceTypeId > 0)
            .Select(item => item.PriceTypeId)
            .Distinct()
            .ToArray();

        var validCategoryIds = await catalogReferenceDataReader
            .GetExistingCategoryIdsAsync(requestedCategoryIds, cancellationToken);

        var validPriceTypeIds = await catalogReferenceDataReader
            .GetExistingPriceTypeIdsAsync(requestedPriceTypeIds, cancellationToken);

        var validRules = new List<OneCCounterpartyCategoryPriceTypeDto>(deduped.Length);
        var skipped = 0;

        foreach (var item in deduped)
        {
            if (!IsValid(item, counterparties, validCategoryIds, validPriceTypeIds, out var reason))
            {
                skipped++;
                logger.LogWarning(
                    "Counterparty category price rule skipped for {CounterpartyId}/{CategoryId}: {Reason}",
                    item.CounterpartyId,
                    item.CategoryId,
                    reason);
                continue;
            }

            validRules.Add(item);
        }

        var existingRules = await salesDbContext.CounterpartyCategoryPriceTypes.ToListAsync(cancellationToken);
        var existingByKey = existingRules.ToDictionary(
            item => (item.CounterpartyId, item.CategoryId),
            item => item,
            CounterpartyCategoryKeyComparer.Instance);

        foreach (var item in validRules)
        {
            var key = (item.CounterpartyId, item.CategoryId);
            if (existingByKey.TryGetValue(key, out var existing))
            {
                if (existing.PriceTypeId != item.PriceTypeId)
                {
                    salesDbContext.Entry(existing).Property(rule => rule.PriceTypeId).CurrentValue = item.PriceTypeId;
                }

                continue;
            }

            await salesDbContext.CounterpartyCategoryPriceTypes.AddAsync(
                CounterpartyCategoryPriceType.Create(item.CounterpartyId, item.CategoryId, item.PriceTypeId),
                cancellationToken);
        }

        var validKeys = validRules
            .Select(item => (item.CounterpartyId, item.CategoryId))
            .ToHashSet(CounterpartyCategoryKeyComparer.Instance);

        var toDelete = existingRules
            .Where(item => !validKeys.Contains((item.CounterpartyId, item.CategoryId)))
            .ToList();

        if (toDelete.Count > 0)
        {
            salesDbContext.CounterpartyCategoryPriceTypes.RemoveRange(toDelete);
        }

        await salesDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "SyncOneCCounterpartyCategoryPriceTypesJob finished. ImportedOrUpdated: {ImportedOrUpdated}, Deleted: {Deleted}, Skipped: {Skipped}",
            validRules.Count,
            toDelete.Count,
            skipped);

        return new CounterpartyCategoryPriceTypesSyncResult(
            ImportedOrUpdated: validRules.Count,
            Deleted: toDelete.Count,
            Skipped: skipped);
    }

    private static bool IsValid(
        OneCCounterpartyCategoryPriceTypeDto item,
        HashSet<string> counterparties,
        HashSet<int> validCategoryIds,
        HashSet<int> validPriceTypeIds,
        out string reason)
    {
        if (string.IsNullOrWhiteSpace(item.CounterpartyId))
        {
            reason = "CounterpartyId is empty.";
            return false;
        }

        if (!counterparties.Contains(item.CounterpartyId))
        {
            reason = "Counterparty is missing in local Sales database.";
            return false;
        }

        if (item.CategoryId <= 0)
        {
            reason = $"CategoryId is invalid: {item.CategoryId}.";
            return false;
        }

        if (!validCategoryIds.Contains(item.CategoryId))
        {
            reason = $"CategoryId {item.CategoryId} does not exist in Catalog.";
            return false;
        }

        if (item.PriceTypeId <= 0)
        {
            reason = $"PriceTypeId is invalid: {item.PriceTypeId}.";
            return false;
        }

        if (!validPriceTypeIds.Contains(item.PriceTypeId))
        {
            reason = $"PriceTypeId {item.PriceTypeId} does not exist in Catalog.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private sealed class CounterpartyCategoryKeyComparer : IEqualityComparer<(string CounterpartyId, int CategoryId)>
    {
        public static readonly CounterpartyCategoryKeyComparer Instance = new();

        public bool Equals((string CounterpartyId, int CategoryId) x, (string CounterpartyId, int CategoryId) y)
            => StringComparer.Ordinal.Equals(x.CounterpartyId, y.CounterpartyId) && x.CategoryId == y.CategoryId;

        public int GetHashCode((string CounterpartyId, int CategoryId) obj)
            => HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.CounterpartyId), obj.CategoryId);
    }
}
