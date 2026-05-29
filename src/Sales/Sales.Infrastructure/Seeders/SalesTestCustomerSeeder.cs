namespace Sales.Infrastructure.Seeders;

public sealed class SalesTestCustomerSeeder(
    SalesDbContext salesDbContext,
    IOptions<SalesTestCustomerOptions> testCustomerOptions,
    ILogger<SalesTestCustomerSeeder> logger) : ISeeder
{
    private readonly SalesTestCustomerOptions _testCustomerOptions = testCustomerOptions.Value;

    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_testCustomerOptions.IdentityUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(_testCustomerOptions.CounterpartyId))
        {
            logger.LogWarning("Sales test customer seeding is skipped because test customer options are incomplete.");
            return;
        }

        await AddCounterpartyIfMissingAsync(cancellationToken);
        await AddCategoryPriceTypeIfMissingAsync(categoryId: 1707, priceTypeId: 5, cancellationToken);
        await AddCategoryPriceTypeIfMissingAsync(categoryId: 4457, priceTypeId: 4, cancellationToken);
        await AddCategoryPriceTypeIfMissingAsync(categoryId: 6139, priceTypeId: 6, cancellationToken);

        await salesDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded Sales test customer {CounterpartyId} for Identity user {IdentityUserId}.",
            _testCustomerOptions.CounterpartyId,
            _testCustomerOptions.IdentityUserId);
    }

    private async Task AddCounterpartyIfMissingAsync(CancellationToken cancellationToken)
    {
        var counterpartyExists = await salesDbContext.Counterparties
            .IgnoreQueryFilters()
            .AnyAsync(counterparty => counterparty.Id == _testCustomerOptions.CounterpartyId, cancellationToken);

        if (counterpartyExists)
        {
            return;
        }

        var counterparty = Counterparty.Create(
            id: _testCustomerOptions.CounterpartyId,
            identityUserId: _testCustomerOptions.IdentityUserId,
            name: _testCustomerOptions.FullName,
            email: _testCustomerOptions.Email,
            phone: null,
            defaultPriceTypeId: 10,
            createdAt: DateTimeOffset.UtcNow);

        await salesDbContext.Counterparties.AddAsync(counterparty, cancellationToken);
    }

    private async Task AddCategoryPriceTypeIfMissingAsync(
        int categoryId,
        int priceTypeId,
        CancellationToken cancellationToken)
    {
        var exists = await salesDbContext.CounterpartyCategoryPriceTypes
            .AnyAsync(rule =>
                rule.CounterpartyId == _testCustomerOptions.CounterpartyId &&
                rule.CategoryId == categoryId,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var rule = CounterpartyCategoryPriceType.Create(
            _testCustomerOptions.CounterpartyId,
            categoryId,
            priceTypeId);

        await salesDbContext.CounterpartyCategoryPriceTypes.AddAsync(rule, cancellationToken);
    }
}
