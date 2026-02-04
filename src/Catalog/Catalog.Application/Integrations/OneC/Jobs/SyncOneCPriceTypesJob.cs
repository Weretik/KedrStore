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
    private async Task CreateOrUpsertPriceTypesAsync(IReadOnlyList<OneCPriceTypeDto> priceTypeDtos, CancellationToken cancellationToken)
    {
        // Получаем все существующие ID одним запросом, чтобы избежать проблемы параллельных запусков или дублей
        var existingPriceTypes = await priceTypeRepo.ListAsync(cancellationToken);
        var existingIds = existingPriceTypes.Select(x => x.Id).ToHashSet();

        foreach (var item in priceTypeDtos)
        {
            var priceTypeId = PriceTypeId.From(item.PriceTypeId);

            if (!existingIds.Contains(priceTypeId))
            {
                var priceType = PriceType.Create(
                    id: priceTypeId,
                    priceTypeName: item.PriceTypeName);

                await priceTypeRepo.AddAsync(priceType, cancellationToken);
                existingIds.Add(priceTypeId); // Добавляем в локальный кэш, чтобы не добавить дважды из одного списка 1С
                logger.LogInformation("[DEBUG_LOG] Prepared new PriceType: {Name} (ID: {Id})", item.PriceTypeName, item.PriceTypeId);
            }
            else
            {
                var existing = existingPriceTypes.First(x => x.Id == priceTypeId);
                existing.UpdateName(item.PriceTypeName);
            }
        }

        await priceTypeRepo.SaveChangesAsync(cancellationToken);
    }

}
