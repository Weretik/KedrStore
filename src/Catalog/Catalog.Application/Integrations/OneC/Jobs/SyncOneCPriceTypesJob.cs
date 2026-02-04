using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public class SyncOneCPriceTypesJob(IOneCClient oneC, ICatalogRepository<PriceType> priceTypeRepo,
    ILogger<SyncOneCPricesJob> logger)
{
    [DisableConcurrentExecution(15 * 60)]
    public async Task RunAsync(IJobCancellationToken jobCancellationToken)
    {
        var cancellationToken = jobCancellationToken.ShutdownToken;

        logger.LogInformation("[DEBUG_LOG] SyncOneCPriceTypesJob started");

        var pricesTypesOneC = await oneC.GetPriceTypesAsync(cancellationToken);
        logger.LogInformation("[DEBUG_LOG] Received {Count} price types from 1C", pricesTypesOneC.Count);

        if (pricesTypesOneC.Count == 0)
        {
            logger.LogWarning("[DEBUG_LOG] No price types received from 1C. Sync stopping.");
            return;
        }

        await CreateOrUpsertPriceTypesAsync(pricesTypesOneC, cancellationToken);

        logger.LogInformation("SyncOneCPriceTypesJob finished");
    }
    private async Task CreateOrUpsertPriceTypesAsync(IReadOnlyList<OneCPriceTypeDto> priceTypeDtos,CancellationToken cancellationToken)
    {
        foreach (var item in priceTypeDtos)
        {
            var priceTypeId = PriceTypeId.From(item.PriceTypeId);
            var existing = await priceTypeRepo.GetByIdAsync(priceTypeId, cancellationToken);
            if (existing is null)
            {
                var priceType = PriceType.Create(
                    id: priceTypeId,
                    priceTypeName: item.PriceTypeName);

                await priceTypeRepo.AddAsync(priceType, cancellationToken);
            }
            else
            {
                existing.UpdateName(item.PriceTypeName);
            }
        }
        await priceTypeRepo.SaveChangesAsync(cancellationToken);
    }

}
