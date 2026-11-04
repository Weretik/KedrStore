using IntegrationTests.TestSupport.Database;
using Media.Application.Contracts.Storage;
using Media.Application.Features.Files.GetList;
using Media.Application.Features.Folders.GetTree;
using Media.Application.Jobs.CdnSync;
using Media.Application.Contracts.Persistence;
using Media.Contracts.Files;
using Media.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests.Media;

/// <summary>Schema, CDN sync, and list queries of the Media module against real PostgreSQL.</summary>
[Collection(MediaPostgresCollection.Name)]
public sealed class MediaPostgresTests(MediaPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Migrations_CreateMediaSchemaWithOwnHistory()
    {
        postgres.SkipIfUnavailable();
        await using var db = postgres.CreateContext();

        var history = await db.Database
            .SqlQueryRaw<string>("""SELECT "MigrationId" AS "Value" FROM media."__EFMigrationsHistory" """)
            .ToListAsync();
        Assert.Contains(history, id => id.EndsWith("_InitialMedia", StringComparison.Ordinal));
    }

    [SkippableFact]
    public async Task CdnSync_DryRunSavesNothing_ThenRegistersFoldersOnce()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        var cdn = new FakeMediaStorage();
        foreach (var key in new[]
                 {
                     "products/731.jpg", "products/732.JPG", "products/old/733.png", "product-scheme/s731.jpg",
                     "products/readme.txt", "avatars/u1.webp", "library/2026/11/orphan.webp", "banners/main.webp",
                     ".thumbs/fc33be99/731.jpg", "products/.cache/731.jpg"
                 })
            cdn.Files[key] = [1, 2, 3];
        await using var provider = BuildProvider(cdn);

        var dry = await RunSync(provider, dryRun: true);
        Assert.Equal((7, 0, 5, 1, 1, 2), (dry.FoundOnCdn, dry.AlreadyKnown, dry.Registered, dry.NotPictures, dry.Orphans, dry.HiddenSkipped));
        Assert.Equal(3, dry.RegisteredByFolder["Товари"] + dry.RegisteredByFolder["Товари / old"]);
        await using (var db = postgres.CreateContext())
            Assert.Equal(0, await db.MediaFiles.CountAsync());

        var applied = await RunSync(provider, dryRun: false);
        Assert.Equal(5, applied.Registered);
        var again = await RunSync(provider, dryRun: false);
        Assert.Equal((0, 5), (again.Registered, again.AlreadyKnown));

        cdn.Files.TryRemove("banners/main.webp", out _);
        Assert.Equal(1, (await RunSync(provider, dryRun: true)).MissingOnCdn);

        await using var scope = provider.CreateAsyncScope();
        var read = scope.ServiceProvider.GetRequiredService<IReadMediaDbContext>();
        var tree = (await new GetMediaFolderTreeQueryHandler(read).Handle(new GetMediaFolderTreeQuery(), default)).Value;
        Assert.Equal(["banners", "old", "Схеми", "Товари"], tree.Folders.Select(folder => folder.Name).Order(StringComparer.Ordinal));
        var products = tree.Folders.Single(folder => folder.Name == "Товари");
        Assert.Equal(2, products.FileCount);
        Assert.Equal(products.Id, tree.Folders.Single(folder => folder.Name == "old").ParentId);
        Assert.Equal(5, tree.TotalFileCount);

        var urls = scope.ServiceProvider.GetRequiredService<IMediaUrlBuilder>();
        var page = (await new GetMediaFilesQueryHandler(read, urls)
            .Handle(new GetMediaFilesQuery(null, false, "73", 1, 2), default)).Value;
        Assert.Equal(4, page.TotalCount);
        Assert.Equal(["731.jpg", "732.JPG"], page.Items.Select(item => item.FileName));
        Assert.Equal("https://images-kedr.cdn.express/products/731.jpg", page.Items[0].Url);

        // The Media contract for other modules: files directly in a CDN directory, and by ID.
        var directory = scope.ServiceProvider.GetRequiredService<IMediaFileDirectory>();
        var inProducts = await directory.ListInStorageDirectoryAsync("products", default);
        Assert.Equal(["products/731.jpg", "products/732.JPG"], inProducts.Select(file => file.StorageKey).Order(StringComparer.Ordinal));
        var byId = await directory.GetByIdsAsync([inProducts[0].Id, 999_999], default);
        Assert.Equal(inProducts[0].Url, Assert.Single(byId).Url);
    }

    private static async Task<MediaCdnSyncReport> RunSync(ServiceProvider provider, bool dryRun)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<SyncCdnMediaJob>().RunAsync(dryRun, default);
    }

    private async Task ResetAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("""DELETE FROM media."MediaFiles"; DELETE FROM media."MediaFolders";""");
    }

    private ServiceProvider BuildProvider(FakeMediaStorage cdn)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = postgres.ConnectionString,
                ["MediaStorage:PublicBaseUrl"] = "https://images-kedr.cdn.express"
            })
            .Build();
        var services = new ServiceCollection().AddMediaInfrastructureServices(configuration);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.RemoveAll<IMediaStorage>();
        services.AddSingleton<IMediaStorage>(cdn);
        return services.BuildServiceProvider();
    }
}
