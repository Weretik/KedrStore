using IntegrationTests.TestSupport.Database;
using Media.Application.Contracts.Storage;
using Media.Application.Features.Files.GetList;
using Media.Application.Features.Folders.GetTree;
using Media.Application.Jobs.CdnSync;
using Media.Application.Contracts.Persistence;
using Media.Contracts.Files;
using Media.Contracts.Folders;
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

    [SkippableFact]
    public async Task Lookup_GivesOtherModulesTheUrlOfKnownFilesOnly()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        var cdn = new FakeMediaStorage();
        cdn.Files["products/731.jpg"] = [1, 2, 3];
        await using var provider = BuildProvider(cdn);
        await RunSync(provider, dryRun: false);
        await using var scope = provider.CreateAsyncScope();
        var id = await scope.ServiceProvider.GetRequiredService<IReadMediaDbContext>().MediaFiles
            .Select(file => file.Id.Value).SingleAsync();

        var files = await scope.ServiceProvider.GetRequiredService<IMediaFileLookup>()
            .GetFilesAsync([id, id + 1000], default);

        var file = Assert.Single(files).Value;
        Assert.Equal(id, file.Id);
        Assert.Equal("731.jpg", file.FileName);
        Assert.Equal("https://images-kedr.cdn.express/products/731.jpg", file.Url);
    }

    [SkippableFact]
    public async Task Arranger_BuildsTheTreeInsideTheTopFolder_AndLeavesFilesPutElsewhere()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        var cdn = new FakeMediaStorage();
        foreach (var key in new[] { "products/731.jpg", "products/732.jpg", "products/old/733.jpg", "banners/734.jpg" })
            cdn.Files[key] = [1, 2, 3];
        await using var provider = BuildProvider(cdn);
        await RunSync(provider, dryRun: false);
        Dictionary<string, long> ids;
        await using (var db = postgres.CreateContext())
            ids = await db.MediaFiles.ToDictionaryAsync(file => file.StorageKey, file => file.Id.Value);

        MediaFolderTreeNode[] tree =
        [
            new(1, "Фурнітура", null), new(2, "Завіси", 1), new(3, "Ручки", 1), new(4, "Порожня", 1)
        ];
        MediaFilePlacement[] placements =
        [
            new(ids["products/731.jpg"], 2), new(ids["products/732.jpg"], 3), new(ids["products/old/733.jpg"], 2),
            new(ids["banners/734.jpg"], 3), new(999_999, 2)
        ];

        var dry = await ArrangeAsync(provider, tree, placements, dryRun: true);
        Assert.Equal(new MediaFolderArrangeResult(3, 3, 1), dry);
        await using (var db = postgres.CreateContext())
            Assert.Equal(3, await db.MediaFolders.CountAsync()); // Товари, old, banners from the sync

        Assert.Equal(new MediaFolderArrangeResult(3, 3, 1), await ArrangeAsync(provider, tree, placements, dryRun: false));
        Assert.Equal(new MediaFolderArrangeResult(0, 0, 1), await ArrangeAsync(provider, tree, placements, dryRun: false));

        await using (var db = postgres.CreateContext())
        {
            var folders = await db.MediaFolders.AsNoTracking().ToListAsync();
            string PathOf(long? id)
            {
                var names = new List<string>();
                for (var folder = folders.SingleOrDefault(f => f.Id.Value == id); folder is not null;
                     folder = folders.SingleOrDefault(f => f.Id == folder.ParentId))
                    names.Insert(0, folder.Name);
                return string.Join(" / ", names);
            }

            var files = await db.MediaFiles.AsNoTracking().ToListAsync();
            string FolderOf(string key) => PathOf(files.Single(file => file.StorageKey == key).FolderId?.Value);
            Assert.Equal("Товари / Фурнітура / Завіси", FolderOf("products/731.jpg"));
            Assert.Equal("Товари / Фурнітура / Ручки", FolderOf("products/732.jpg"));
            Assert.Equal("Товари / Фурнітура / Завіси", FolderOf("products/old/733.jpg"));
            Assert.Equal("banners", FolderOf("banners/734.jpg"));
            Assert.DoesNotContain(folders, folder => folder.Name == "Порожня");
        }
    }

    private static async Task<MediaFolderArrangeResult> ArrangeAsync(ServiceProvider provider,
        MediaFolderTreeNode[] tree, MediaFilePlacement[] placements, bool dryRun)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediaFolderArranger>()
            .ArrangeAsync("products", tree, placements, dryRun, default);
    }

    private static async Task<MediaCdnSyncReport> RunSync(ServiceProvider provider, bool dryRun)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<SyncCdnMediaJob>().RunAsync(dryRun, default);
    }

    [SkippableFact]
    public async Task Uploader_PutsFilesUnderTheDirectory_IntoOneRootFolder()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        var cdn = new FakeMediaStorage();
        await using var provider = BuildProvider(cdn);
        var picture = IntegrationTests.Identity.Avatars.TestImages.Create(1200, 600);

        MediaFileUploadResult first, second;
        await using (var scope = provider.CreateAsyncScope())
        {
            var uploader = scope.ServiceProvider.GetRequiredService<IMediaFileUploader>();
            first = await uploader.UploadAsync(new MediaFileUploadRequest("articles", "Статті", "a.jpg", picture), default);
            second = await uploader.UploadAsync(new MediaFileUploadRequest("/articles/", "статті", "b.jpg", picture), default);
            var text = await uploader.UploadAsync(new MediaFileUploadRequest("articles", "Статті", "c.txt", [1, 2, 3]), default);
            Assert.Equal(MediaFileUploadStatus.NotAPicture, text.Status);
        }

        Assert.Equal(MediaFileUploadStatus.Uploaded, first.Status);
        Assert.StartsWith("articles/", first.File!.StorageKey, StringComparison.Ordinal);
        Assert.Equal($"https://images-kedr.cdn.express/{first.File.StorageKey}", first.File.Url);
        Assert.Equal(2, cdn.Files.Count);
        await using var db = postgres.CreateContext();
        var folder = Assert.Single(await db.MediaFolders.ToListAsync());
        Assert.Equal("Статті", folder.Name);
        Assert.Equal(2, await db.MediaFiles.CountAsync(file => file.FolderId == folder.Id));
        Assert.NotEqual(first.File.Id, second.File!.Id);

        await using var offline = BuildProvider(new FakeMediaStorage(isConfigured: false));
        await using var offlineScope = offline.CreateAsyncScope();
        var unavailable = await offlineScope.ServiceProvider.GetRequiredService<IMediaFileUploader>()
            .UploadAsync(new MediaFileUploadRequest("articles", "Статті", "a.jpg", picture), default);
        Assert.Equal(MediaFileUploadStatus.StorageUnavailable, unavailable.Status);
        await ResetAsync();
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
