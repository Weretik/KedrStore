using Catalog.Domain.Entities;
using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Domain.Abstractions;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Projections;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Jobs;
using Catalog.Application.Integrations.OneC.Options;
using Catalog.Application.Integrations.OneC.Sync;
using Catalog.Application.Slugs;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Catalog.Import;

/// <summary>In-memory catalog and fakes for running the 1C import jobs without 1C or PostgreSQL.</summary>
internal static class CatalogImportTestSupport
{
    public const string Root = "000005513";

    public static CatalogDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CatalogDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public static SyncOneCProductDetailsJob ProductJob(CatalogDbContext db, FakeOneC oneC)
        => new(
            oneC,
            new TestRepository<Product>(db),
            new TestRepository<ProductCategory>(db),
            new SlugService(db),
            new ProductTranslationImporter(new TestRepository<ProductTranslation>(db), NullLogger<ProductTranslationImporter>.Instance),
            new OptionsSnapshot<RootCategoryId>(Options()),
            new NoopProjectionRebuilder(),
            new NoopPhotoCheckStore(),
            NullLogger<SyncOneCProductDetailsJob>.Instance);

    public static SyncOneCCategoryJob CategoryJob(CatalogDbContext db, FakeOneC oneC)
        => new(
            oneC,
            new TestRepository<ProductCategory>(db),
            new SlugService(db),
            new OptionsSnapshot<RootCategoryId>(Options()),
            new OptionsSnapshot<CategoryPresentationOptions>(new CategoryPresentationOptions()),
            NullLogger<SyncOneCCategoryJob>.Instance);

    private static RootCategoryId Options() => new()
    {
        DoorsRootCategoryId = "000007226",
        HardwareRootCategoryId = Root,
        CosmosRootCategoryId = "Космос",
        CosmosCategoryId = 920000
    };

    public sealed class TestRepository<T>(CatalogDbContext db) : RepositoryBase<T>(db), ICatalogRepository<T>
        where T : class, IAggregateRoot;

    public sealed class FakeOneC : IOneCClient
    {
        public List<OneCCategoryDto> Categories { get; set; } = [];
        public List<OneCProductDto> Products { get; set; } = [];

        public Task<IReadOnlyList<OneCCategoryDto>> GetCategoriesAsync(string rootCategoryId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCCategoryDto>>(Categories);

        public Task<IReadOnlyList<OneCProductDto>> GetProductDetailsAsync(string rootCategoryId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OneCProductDto>>(Products);

        public Task<IReadOnlyList<OneCStockDto>> GetProductStocksAsync(string rootCategoryId, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<OneCPriceDto>> GetProductPricesAsync(string rootCategoryId, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<OneCPriceTypeDto>> GetPriceTypesAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class OptionsSnapshot<T>(T value) : IOptionsSnapshot<T> where T : class
    {
        public T Value => value;
        public T Get(string? name) => value;
    }

    private sealed class NoopProjectionRebuilder : IProductListProjectionRebuilder
    {
        public Task RebuildAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class NoopPhotoCheckStore : IProductPhotoCheckStore
    {
        public Task<IReadOnlyList<ProductPhotoCandidate>> GetActiveProductBatchAsync(int afterProductId, int take,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task ReconcileAsync(IReadOnlyCollection<ProductPhotoCandidate> products, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<int> SaveResultsAsync(IReadOnlyCollection<ProductPhotoCheckUpdate> updates, DateTimeOffset checkedAtUtc,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
