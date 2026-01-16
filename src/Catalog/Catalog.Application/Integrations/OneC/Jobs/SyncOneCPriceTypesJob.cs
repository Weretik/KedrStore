using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Persistence;
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

        logger.LogInformation("SyncOneCPriceTypesJob started for");

        var pricesTypesOneC = await oneC.GetPriceTypesAsync(cancellationToken);

        if (pricesTypesOneC.Count == 0)
            return;

        await CreateOrUpsertPriceTypesAsync(pricesTypesOneC, cancellationToken);

        logger.LogInformation("SyncOneCPriceTypesJob finished for");
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
