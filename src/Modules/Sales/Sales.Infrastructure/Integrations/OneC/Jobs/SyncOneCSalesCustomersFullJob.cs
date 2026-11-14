namespace Sales.Infrastructure.Integrations.OneC.Jobs;

public sealed class SyncOneCSalesCustomersFullJob(
    SyncOneCCounterpartiesJob counterpartiesJob,
    SyncOneCCounterpartyCategoryPriceTypesJob counterpartyCategoryPriceTypesJob,
    ILogger<SyncOneCSalesCustomersFullJob> logger)
{
    public async Task<SalesCustomersFullSyncResult> RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncOneCSalesCustomersFullJob started");

        var counterpartiesResult = await counterpartiesJob.RunAsync(cancellationToken);
        var priceRulesResult = await counterpartyCategoryPriceTypesJob.RunAsync(cancellationToken);

        logger.LogInformation(
            "SyncOneCSalesCustomersFullJob finished. Counterparties: Imported={Imported}, Updated={Updated}, Restored={Restored}, Deleted={Deleted}, Skipped={Skipped}. PriceRules: ImportedOrUpdated={ImportedOrUpdated}, Deleted={PriceRulesDeleted}, Skipped={PriceRulesSkipped}",
            counterpartiesResult.Imported,
            counterpartiesResult.Updated,
            counterpartiesResult.Restored,
            counterpartiesResult.Deleted,
            counterpartiesResult.Skipped,
            priceRulesResult.ImportedOrUpdated,
            priceRulesResult.Deleted,
            priceRulesResult.Skipped);

        return new SalesCustomersFullSyncResult(counterpartiesResult, priceRulesResult);
    }
}
