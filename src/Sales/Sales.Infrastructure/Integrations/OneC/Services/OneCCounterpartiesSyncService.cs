using Ardalis.Result;
using Identity.Application.Services;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Customers.Entities;

namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed class OneCCounterpartiesSyncService(
    ISalesOneCReadClient oneCClient,
    SalesDbContext salesDbContext,
    IImportedCounterpartyIdentityProvisioningService identityProvisioningService,
    CounterpartyContactNormalizer contactNormalizer,
    ILogger<OneCCounterpartiesSyncService> logger)
{
    public async Task<CounterpartiesSyncResult> RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCCounterpartiesJob started");

        var counterparties = await oneCClient.GetCounterpartiesAsync(cancellationToken);
        if (counterparties.Count == 0)
        {
            logger.LogWarning("No counterparties received from 1C. Sync stopping.");
            return new CounterpartiesSyncResult(0, 0, 0, 0, 0);
        }

        var deduped = counterparties
            .Where(item => !string.IsNullOrWhiteSpace(item.CounterpartyId))
            .GroupBy(item => item.CounterpartyId.Trim(), StringComparer.Ordinal)
            .Select(group => group.Last() with { CounterpartyId = group.Key })
            .ToArray();

        var existingCounterparties = await salesDbContext.Counterparties
            .IgnoreQueryFilters()
            .ToDictionaryAsync(item => item.Id, StringComparer.Ordinal, cancellationToken);
        var assignedIdentityUsers = existingCounterparties.Values
            .GroupBy(item => item.IdentityUserId)
            .ToDictionary(group => group.Key, group => group.First().Id);

        var imported = 0;
        var updated = 0;
        var restored = 0;
        var deleted = 0;
        var skipped = 0;
        var activeCounterpartyIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in deduped)
        {
            if (!TryNormalize(item, out var normalized, out var reason))
            {
                skipped++;
                logger.LogWarning(
                    "Counterparty {CounterpartyId} skipped during sync: {Reason}",
                    item.CounterpartyId,
                    reason);
                continue;
            }

            existingCounterparties.TryGetValue(normalized.CounterpartyId, out var existingCounterparty);

            var identityResult = await identityProvisioningService.EnsureUserAsync(
                new ImportedCounterpartyIdentityUserData(
                    normalized.CounterpartyId,
                    normalized.Email,
                    normalized.CounterpartyName),
                existingCounterparty?.IdentityUserId,
                cancellationToken);

            if (!identityResult.IsSuccess)
            {
                skipped++;
                logger.LogWarning(
                    "Counterparty {CounterpartyId} skipped because identity provisioning failed: {Errors}",
                    normalized.CounterpartyId,
                    JoinErrors(identityResult));
                continue;
            }

            if (assignedIdentityUsers.TryGetValue(identityResult.Value, out var assignedCounterpartyId)
                && !string.Equals(assignedCounterpartyId, normalized.CounterpartyId, StringComparison.Ordinal))
            {
                skipped++;
                logger.LogWarning(
                    "Counterparty {CounterpartyId} skipped because IdentityUserId {IdentityUserId} is already linked to counterparty {AssignedCounterpartyId}.",
                    normalized.CounterpartyId,
                    identityResult.Value,
                    assignedCounterpartyId);
                continue;
            }

            activeCounterpartyIds.Add(normalized.CounterpartyId);
            var now = DateTimeOffset.UtcNow;

            if (existingCounterparty is null)
            {
                var counterparty = Counterparty.Create(
                    id: normalized.CounterpartyId,
                    identityUserId: identityResult.Value,
                    name: normalized.CounterpartyName,
                    email: normalized.Email,
                    phone: normalized.Phone,
                    defaultPriceTypeId: normalized.DefaultPriceTypeId,
                    createdAt: now);

                await salesDbContext.Counterparties.AddAsync(counterparty, cancellationToken);
                existingCounterparties[normalized.CounterpartyId] = counterparty;
                assignedIdentityUsers[identityResult.Value] = normalized.CounterpartyId;
                imported++;
                continue;
            }

            if (existingCounterparty.IsDeleted)
            {
                existingCounterparty.Restore(now);
                restored++;
            }

            existingCounterparty.Update(
                identityUserId: identityResult.Value,
                name: normalized.CounterpartyName,
                email: normalized.Email,
                phone: normalized.Phone,
                defaultPriceTypeId: normalized.DefaultPriceTypeId,
                updatedAt: now);

            assignedIdentityUsers[identityResult.Value] = normalized.CounterpartyId;
            updated++;
        }

        var staleCounterparties = existingCounterparties.Values
            .Where(item => !activeCounterpartyIds.Contains(item.Id))
            .ToArray();

        if (staleCounterparties.Length > 0)
        {
            var staleCounterpartyIds = staleCounterparties
                .Select(item => item.Id)
                .ToHashSet(StringComparer.Ordinal);

            var staleRules = await salesDbContext.CounterpartyCategoryPriceTypes
                .Where(rule => staleCounterpartyIds.Contains(rule.CounterpartyId))
                .ToListAsync(cancellationToken);

            if (staleRules.Count > 0)
            {
                salesDbContext.CounterpartyCategoryPriceTypes.RemoveRange(staleRules);
            }

            salesDbContext.Counterparties.RemoveRange(staleCounterparties);
            deleted = staleCounterparties.Length;
        }

        await salesDbContext.SaveChangesAsync(cancellationToken);

        foreach (var staleCounterparty in staleCounterparties)
        {
            var deleteIdentityResult = await identityProvisioningService.DeleteUserAsync(
                staleCounterparty.IdentityUserId,
                staleCounterparty.Id,
                cancellationToken);

            if (!deleteIdentityResult.IsSuccess)
            {
                logger.LogWarning(
                    "Counterparty {CounterpartyId} was deleted from Sales, but deleting linked AppUser {IdentityUserId} failed: {Errors}",
                    staleCounterparty.Id,
                    staleCounterparty.IdentityUserId,
                    string.Join(", ", deleteIdentityResult.Errors));
            }
        }

        logger.LogInformation(
            "SyncOneCCounterpartiesJob finished. Imported: {Imported}, Updated: {Updated}, Restored: {Restored}, Deleted: {Deleted}, Skipped: {Skipped}",
            imported,
            updated,
            restored,
            deleted,
            skipped);

        return new CounterpartiesSyncResult(
            Imported: imported,
            Updated: updated,
            Restored: restored,
            Deleted: deleted,
            Skipped: skipped);
    }

    private bool TryNormalize(
        OneCCounterpartyDto item,
        out NormalizedCounterparty normalized,
        out string reason)
    {
        normalized = default!;

        if (string.IsNullOrWhiteSpace(item.CounterpartyId))
        {
            reason = "CounterpartyId is empty.";
            return false;
        }

        var counterpartyId = item.CounterpartyId.Trim();

        if (string.IsNullOrWhiteSpace(item.CounterpartyName))
        {
            reason = "CounterpartyName is empty.";
            return false;
        }

        var counterpartyName = item.CounterpartyName.Trim();

        if (item.DefaultPriceTypeId <= 0)
        {
            reason = $"DefaultPriceTypeId is invalid: {item.DefaultPriceTypeId}.";
            return false;
        }

        if (!contactNormalizer.TryNormalizeEmail(item.Email, out var normalizedEmail))
        {
            reason = "Email is missing or invalid. Current site authorization requires a valid email.";
            return false;
        }

        string? normalizedPhone = null;
        if (!string.IsNullOrWhiteSpace(item.Phone)
            && !contactNormalizer.TryNormalizePhone(item.Phone, out normalizedPhone))
        {
            logger.LogWarning(
                "Counterparty {CounterpartyId} phone was ignored because it is invalid: {Phone}",
                counterpartyId,
                item.Phone);
        }

        normalized = new NormalizedCounterparty(
            counterpartyId,
            counterpartyName,
            normalizedEmail!,
            normalizedPhone,
            item.DefaultPriceTypeId);

        reason = string.Empty;
        return true;
    }

    private static string JoinErrors(Result<Guid> result)
        => string.Join(", ", result.Errors);

    private sealed record NormalizedCounterparty(
        string CounterpartyId,
        string CounterpartyName,
        string Email,
        string? Phone,
        int DefaultPriceTypeId);
}
