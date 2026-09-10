using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList.DTOs;
using Sales.Domain.Customers.Entities;
using Sales.Infrastructure.DependencyInjection;

namespace IntegrationTests.Sales.Persistence;

[Collection(SalesPostgresCollection.Name)]
public sealed class SalesCustomerReadModelTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task GetListAsync_ReturnsOnlyActiveCustomersOrderedByNameAndId()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var now = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        await SeedCustomerAsync("customer-b", "Bravo", now);
        await SeedCustomerAsync("customer-a2", "Alpha", now);
        await SeedCustomerAsync("customer-a1", "Alpha", now);
        await SeedCustomerAsync("customer-deleted", "Deleted", now, deleted: true);

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<ICustomerReadService>();

        var result = await reader.GetListAsync(
            new CustomerListRequest { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.PagedInfo.TotalRecords);
        Assert.Equal(
            ["customer-a1", "customer-a2", "customer-b"],
            result.Value.Value.Select(customer => customer.CounterpartyId));
        Assert.DoesNotContain(typeof(CustomerListItemDto).GetProperties(), property =>
            property.Name is "Email" or "IdentityUserId" or "IsDeleted");
        Assert.Empty(scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.DataBase.SalesDbContext>()
            .ChangeTracker.Entries());
    }

    [SkippableFact]
    public async Task GetListAsync_AppliesPagingAndReturnsEmptyPagePastTheEnd()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var now = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        await SeedCustomersAsync(125, now);

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<ICustomerReadService>();

        var page = await reader.GetListAsync(
            new CustomerListRequest { Page = 3, PageSize = 50 },
            CancellationToken.None);
        var pagePastEnd = await reader.GetListAsync(
            new CustomerListRequest { Page = 4, PageSize = 50 },
            CancellationToken.None);

        Assert.True(page.IsSuccess);
        Assert.Equal(125, page.Value.PagedInfo.TotalRecords);
        Assert.Equal(3, page.Value.PagedInfo.TotalPages);
        Assert.Equal(25, page.Value.Value.Count);
        Assert.Equal("customer-read-100", page.Value.Value[0].CounterpartyId);
        Assert.True(pagePastEnd.IsSuccess);
        Assert.Empty(pagePastEnd.Value.Value);
    }

    [SkippableFact]
    public async Task GetByIdAsync_ReturnsActiveCustomerAndOrderedCategoryPriceTypes()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var now = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        await SeedCustomerAsync("customer-detail", "Customer detail", now, phone: null);
        await using (var db = postgres.CreateContext())
        {
            db.CounterpartyCategoryPriceTypes.AddRange(
                CounterpartyCategoryPriceType.Create("customer-detail", 20, 202),
                CounterpartyCategoryPriceType.Create("customer-detail", 10, 101));
            await db.SaveChangesAsync();
        }

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICustomerReadService>()
            .GetByIdAsync("customer-detail", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("customer-detail", result.Value.CounterpartyId);
        Assert.Equal("Customer detail", result.Value.Name);
        Assert.Null(result.Value.Phone);
        Assert.Equal("customer-detail@example.com", result.Value.Email);
        Assert.Equal(1, result.Value.DefaultPriceTypeId);
        Assert.Equal([10, 20], result.Value.CategoryPriceTypes.Select(rule => rule.CategoryId));
        Assert.Equal([101, 202], result.Value.CategoryPriceTypes.Select(rule => rule.PriceTypeId));
        Assert.DoesNotContain(typeof(CustomerDetailDto).GetProperties(), property =>
            property.Name is "IdentityUserId" or "IsDeleted" or "CreatedAt" or "UpdatedAt" or "Orders");
        Assert.Empty(scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.DataBase.SalesDbContext>()
            .ChangeTracker.Entries());
    }

    [SkippableFact]
    public async Task GetByIdAsync_ReturnsNotFoundForUnknownAndSoftDeletedCustomers()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var now = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        await SeedCustomerAsync("customer-deleted", "Deleted", now, deleted: true);

        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<ICustomerReadService>();

        var unknown = await reader.GetByIdAsync("customer-unknown", CancellationToken.None);
        var deleted = await reader.GetByIdAsync("customer-deleted", CancellationToken.None);

        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, unknown.Status);
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, deleted.Status);
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = postgres.ConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSalesInfrastructureServices(configuration, includeCatalogReadServices: false);
        return services.BuildServiceProvider();
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderSyncRetryAudits\"");
        await db.OneCOrderSyncs.ExecuteDeleteAsync();
        await db.Orders.ExecuteDeleteAsync();
        await db.CounterpartyCategoryPriceTypes.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    private async Task SeedCustomerAsync(
        string id,
        string name,
        DateTimeOffset now,
        bool deleted = false,
        string? phone = "+380000000000")
    {
        await using var db = postgres.CreateContext();
        var customer = Counterparty.Create(
            id,
            Guid.NewGuid(),
            name,
            $"{id}@example.com",
            phone,
            1,
            now);
        if (deleted)
            customer.MarkAsDeleted(now.AddMinutes(1));

        db.Counterparties.Add(customer);
        await db.SaveChangesAsync();
    }

    private async Task SeedCustomersAsync(int count, DateTimeOffset now)
    {
        await using var db = postgres.CreateContext();
        for (var index = 0; index < count; index++)
        {
            var id = $"customer-read-{index:D3}";
            db.Counterparties.Add(Counterparty.Create(
                id,
                Guid.NewGuid(),
                $"Customer {index:D3}",
                $"{id}@example.com",
                null,
                1,
                now));
        }

        await db.SaveChangesAsync();
    }
}
