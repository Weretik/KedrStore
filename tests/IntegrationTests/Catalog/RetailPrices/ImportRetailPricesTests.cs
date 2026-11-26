using Ardalis.Result;
using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Abstractions.Files;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;
using Catalog.Application.Features.RetailPrices.Commands.UnlockRetailPrices;
using Catalog.Application.Features.RetailPrices.Shared;
using Catalog.Contracts.Pricing;
using Catalog.Domain.Categories;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Prices;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NodaMoney;
using static IntegrationTests.Catalog.Import.CatalogImportTestSupport;

namespace IntegrationTests.Catalog.RetailPrices;

/// <summary>
/// Spec catalog/010 SC-RP03: the owner's file changes only the retail price type, by product code, locks the prices
/// it brought from the 1C sync when asked, and reports what it could not use.
/// </summary>
public sealed class ImportRetailPricesTests
{
    private const int Retail = 11;
    private const int Wholesale = 14;
    private const string RetailName = "Розница халтмаг 30%";
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Import_SetsAndLocksFilePrices_LeavesTheRestAlone_AndReports()
    {
        await using var db = CreateContext();
        Seed(db);
        var reader = new FakeSheetReader(
            new RetailPriceSheetRow("1", 201m),
            new RetailPriceSheetRow("2", 341m),
            new RetailPriceSheetRow("3", 120m),
            new RetailPriceSheetRow("999", 15m),
            new RetailPriceSheetRow("", null),
            new RetailPriceSheetRow("5", null));

        var result = await Handler(db, reader).Handle(Command(protect: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RetailName, reader.ColumnName);
        Assert.Equal(2, result.Value.Changed);
        Assert.Equal(1, result.Value.Unchanged);
        Assert.Equal(["999"], result.Value.NotFoundCodes);
        Assert.Equal(1, result.Value.NotFoundCount);
        Assert.Equal(["5"], result.Value.WithoutPriceCodes);
        Assert.Equal(3, result.Value.Status.LockedCount);
        Assert.Equal(Now, result.Value.Status.LastLockedAt);
        Assert.Equal(Retail, result.Value.Status.PriceTypeId);

        var prices = await Prices(db);
        Assert.Equal(
            [(1, Retail, 201m, true), (1, Wholesale, 90m, false), (2, Retail, 341m, true), (3, Retail, 120m, true),
                (4, Retail, 80m, false), (5, Retail, 60m, false)],
            prices);
    }

    [Fact]
    public async Task ImportWithoutProtection_RemovesTheLockOfFilePricesOnly()
    {
        await using var db = CreateContext();
        Seed(db);
        await Handler(db, new FakeSheetReader(new RetailPriceSheetRow("2", 341m), new RetailPriceSheetRow("4", 81m))).Handle(Command(protect: true),
            CancellationToken.None);

        var result = await Handler(db, new FakeSheetReader(new RetailPriceSheetRow("2", 350m))).Handle(Command(protect: false),
            CancellationToken.None);

        Assert.Equal(1, result.Value.Changed);
        Assert.Equal(1, result.Value.Status.LockedCount);
        var prices = await Prices(db);
        Assert.Contains((2, Retail, 350m, false), prices);
        Assert.Contains((4, Retail, 81m, true), prices);
    }

    [Fact]
    public async Task Import_ReturnsTheReadersProblem_AndChangesNothing()
    {
        await using var db = CreateContext();
        Seed(db);
        var reader = new FakeSheetReader { Problem = "У файлі немає колонки «Код»." };

        var result = await Handler(db, reader).Handle(Command(protect: true), CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal("У файлі немає колонки «Код».", Assert.Single(result.ValidationErrors).ErrorMessage);
        Assert.DoesNotContain(await Prices(db), price => price.Locked);
    }

    [Fact]
    public async Task Unlock_ReleasesEveryRetailLock_AndARepeatChangesNothing()
    {
        await using var db = CreateContext();
        Seed(db);
        await Handler(db, new FakeSheetReader(new RetailPriceSheetRow("1", 201m), new RetailPriceSheetRow("2", 341m))).Handle(Command(protect: true),
            CancellationToken.None);
        var unlock = new UnlockRetailPricesHandler(new TestRepository<ProductPrice>(db), db, Status(db),
            NullLogger<UnlockRetailPricesHandler>.Instance);

        var first = await unlock.Handle(new UnlockRetailPricesCommand(), CancellationToken.None);
        var second = await unlock.Handle(new UnlockRetailPricesCommand(), CancellationToken.None);

        Assert.Equal(0, first.Value.LockedCount);
        Assert.Null(first.Value.LastLockedAt);
        Assert.True(second.IsSuccess);
        Assert.Contains((1, Retail, 201m, false), await Prices(db));
    }

    private static ImportRetailPricesHandler Handler(CatalogDbContext db, IRetailPriceSheetReader reader)
        => new(new TestRepository<ProductPrice>(db), db, reader, Status(db), new NoopRebuilder(),
            new FixedClock(), NullLogger<ImportRetailPricesHandler>.Instance);

    private static RetailPriceStatusReader Status(CatalogDbContext db) => new(db, PricingOptions());

    private static IOptions<CatalogPricingOptions> PricingOptions()
        => Options.Create(new CatalogPricingOptions { RetailPriceTypeId = Retail });

    private static ImportRetailPricesCommand Command(bool protect)
        => new(new RetailPriceUpload("Цены_26.11.xlsx", 10, () => new MemoryStream([1])), protect);

    private static async Task<List<(int Product, int Type, decimal Amount, bool Locked)>> Prices(CatalogDbContext db)
        => (await db.ProductPrices.AsNoTracking().ToListAsync())
            .Select(price => (price.ProductId.Value, price.PriceTypeId.Value, price.Amount, price.IsLockedFromOneC))
            .OrderBy(price => price.Item1).ThenBy(price => price.Item2)
            .ToList();

    private static void Seed(CatalogDbContext db)
    {
        db.PriceTypes.Add(PriceType.Create(PriceTypeId.Create(Retail), RetailName));
        db.PriceTypes.Add(PriceType.Create(PriceTypeId.Create(Wholesale), "*ОПТ ГРН"));
        db.Categories.Add(ProductCategory.Create(ProductCategoryId.Create(10), Root, "Замки", "zamky",
            CategoryPath.Create("n10")).Value);
        foreach (var id in new[] { 1, 2, 3, 4, 5 })
        {
            db.Products.Add(Product.Create(ProductId.Create(id), Root, $"Product {id}", $"product-{id}",
                ProductCategoryId.Create(10), DateTimeOffset.UtcNow, 0, 1, false, false, true).Value);
        }

        db.ProductPrices.Add(Price(1, Wholesale, 90));
        db.ProductPrices.Add(Price(2, Retail, 300));
        db.ProductPrices.Add(Price(3, Retail, 120));
        db.ProductPrices.Add(Price(4, Retail, 80));
        db.ProductPrices.Add(Price(5, Retail, 60));
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }

    private static ProductPrice Price(int productId, int priceTypeId, decimal amount)
        => ProductPrice.Create(Root, ProductId.Create(productId), PriceTypeId.Create(priceTypeId),
            new Money(amount, "UAH")).Value;

    private sealed class FakeSheetReader(params RetailPriceSheetRow[] rows) : IRetailPriceSheetReader
    {
        public string? Problem { get; init; }
        public string? ColumnName { get; private set; }

        public Result<RetailPriceSheet> Read(Stream content, string priceColumnName)
        {
            ColumnName = priceColumnName;
            return Problem is null
                ? Result.Success(new RetailPriceSheet(rows))
                : Result<RetailPriceSheet>.Invalid(new ValidationError("file", Problem, null, ValidationSeverity.Error));
        }
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class NoopRebuilder : IProductListProjectionRebuilder
    {
        public Task RebuildAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int> RefreshPhotosAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
