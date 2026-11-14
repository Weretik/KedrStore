using Ardalis.Result;
using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Commands.CreateArticle;
using Content.Application.Features.Articles.Commands.DeleteArticle;
using Content.Application.Features.Articles.Commands.MoveLegacyArticleCovers;
using Content.Application.Features.Articles.Commands.UpdateArticle;
using Content.Application.Features.Articles.Commands.UploadArticleCover;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Queries.GetPublicArticle;
using Content.Application.Features.Articles.Queries.GetArticleCover;
using Content.Application.Features.Articles.Queries.GetPublicArticles;
using Content.Application.Abstractions.Integrations;
using Content.Application.Abstractions.Media;
using Content.Application.Abstractions.Persistence;
using Content.Domain.Articles;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.Identity.Avatars;
using IntegrationTests.TestSupport.Content;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Content.Application.Features.Articles.Shared.Reading;
namespace IntegrationTests.Content.Articles;

/// <summary>Migrations (schema, history table, legacy seed) and article handlers against real PostgreSQL.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class ArticlesPostgresTests(ContentPostgresFixture postgres)
{
    private static readonly DateOnly Day = new(2026, 11, 4);
    private readonly FakeArticleMediaLibrary _media = new();

    [SkippableFact]
    public async Task Migrations_CreateContentSchemaWithSixPublishedLegacyArticles()
    {
        postgres.SkipIfUnavailable();
        await using var db = postgres.CreateContext();

        var history = await db.Database
            .SqlQueryRaw<string>("""SELECT "MigrationId" AS "Value" FROM content."__EFMigrationsHistory" """)
            .ToListAsync();
        Assert.Contains(history, id => id.EndsWith("_SeedLegacyArticles", StringComparison.Ordinal));

        var legacy = (await db.Articles.AsNoTracking().ToListAsync()).Where(a => a.Id.Value <= 6).ToList();
        Assert.Equal(6, legacy.Count);
        Assert.All(legacy, article =>
        {
            Assert.True(article.IsPublished);
            Assert.Equal(1, article.CoverVersion);
            Assert.Equal(2, article.Translations.Count);
        });
        var legacyIds = legacy.Select(article => article.Id).ToList();
        Assert.Equal(6, await db.ArticleCovers.CountAsync(cover => legacyIds.Contains(cover.ArticleId)));
    }

    [SkippableFact]
    public async Task Public_ReadsOnlyPublishedArticlesWithCover()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        await using var provider = BuildProvider();
        var draft = await SendCreate(provider, Input(null, "Чернетка", isPublished: false));

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IReadContentDbContext>();
        var list = await new GetPublicArticlesHandler(db, _media).Handle(new GetPublicArticlesQuery("ru"), default);
        var page = await new GetPublicArticleHandler(db, _media)
            .Handle(new GetPublicArticleQuery("uk", "prykhovani-petli"), default);
        var hidden = await new GetPublicArticleHandler(db, _media)
            .Handle(new GetPublicArticleQuery("uk", draft.Value.Slug), default);
        var cover = await new GetArticleCoverHandler(db).Handle(new GetArticleCoverQuery(4), default);

        Assert.Equal(6, list.Value.Count);
        Assert.DoesNotContain(list.Value, item => item.Id == draft.Value.Id);
        Assert.StartsWith("<p>", page.Value.BodyHtml, StringComparison.Ordinal);
        Assert.Equal(ResultStatus.NotFound, hidden.Status);
        Assert.Equal("image/webp", cover.Value.ContentType);
        Assert.All(list.Value, item => Assert.Equal(1, item.Cover.LegacyVersion));
    }

    [SkippableFact]
    public async Task Create_GeneratesFreeSlugAfterSeededIds_AndUpdateRejectsTakenSlug()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        await using var provider = BuildProvider();

        var first = await SendCreate(provider, Input(null, "Приховані петлі"));
        var second = await SendCreate(provider, Input(null, "Приховані петлі"));

        Assert.True(first.Value.Id > 6);
        Assert.Equal("prykhovani-petli-2", first.Value.Slug);
        Assert.Equal("prykhovani-petli-3", second.Value.Slug);
        Assert.Equal("<p>Текст</p>", first.Value.Texts[0].BodyHtml);
        Assert.Equal(TestContentEditor.EmailAddress, first.Value.UpdatedBy);

        await using var scope = provider.CreateAsyncScope();
        var update = await SendUpdate(provider, second.Value.Id, Input("prykhovani-petli", "Інша"));
        Assert.Equal(ResultStatus.Invalid, update.Status);
        Assert.Equal("slug", Assert.Single(update.ValidationErrors).Identifier);
    }

    [SkippableFact]
    public async Task Cover_UploadedFirst_ThenSavedWithTheNewArticle_AndDeleteRemovesIt()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        await using var provider = BuildProvider();
        var bytes = TestImages.Create(1200, 600);

        Result<UploadedArticleCover> uploaded;
        await using (var scope = provider.CreateAsyncScope())
        {
            uploaded = await new UploadArticleCoverHandler(
                    scope.ServiceProvider.GetRequiredService<ICoverImageProcessor>(), _media)
                .Handle(new UploadArticleCoverCommand(
                    new CoverUpload("Петлі.jpg", "image/jpeg", bytes.Length, () => new MemoryStream(bytes))), default);
        }
        var stored = Assert.Single(_media.Uploads);
        Assert.Equal(("articles", "Статті", "Петлі.webp"), (stored.StorageDirectory, stored.FolderName, stored.FileName));

        var created = await SendCreate(provider, Input(null, "З обкладинкою") with { CoverMediaFileId = uploaded.Value.MediaFileId });
        Assert.Equal(uploaded.Value.MediaFileId, created.Value.Cover.MediaFileId);
        Assert.Equal(uploaded.Value.Url, created.Value.Cover.Url);

        await using (var scope = provider.CreateAsyncScope())
        {
            var deleted = await new DeleteArticleHandler(
                    scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                    scope.ServiceProvider.GetRequiredService<IContentRepository<ArticleCover>>())
                .Handle(new DeleteArticleCommand(created.Value.Id), default);
            Assert.True(deleted.IsSuccess);
        }

        await using var db = postgres.CreateContext();
        Assert.False(await db.Articles.AnyAsync(a => a.Id == ArticleId.Create(created.Value.Id)));
    }

    [SkippableFact]
    public async Task Cover_NewFileReplacesLegacyBytes_UnknownFileIsRefused_RemoveClearsIt()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        await using var provider = BuildProvider();
        var created = await SendCreate(provider, Input(null, "Стара обкладинка"));
        await using (var db = postgres.CreateContext())
        {
            await db.Database.ExecuteSqlRawAsync(
                """UPDATE content."Articles" SET "CoverVersion" = 1 WHERE "Id" = {0}""", created.Value.Id);
            await db.Database.ExecuteSqlRawAsync(
                """INSERT INTO content."ArticleCovers" ("ArticleId", "Data", "ContentType") VALUES ({0}, {1}, 'image/webp')""",
                created.Value.Id, new byte[] { 1, 2, 3 });
        }

        var unknown = await SendUpdate(provider, created.Value.Id,
            Input(created.Value.Slug, "Стара обкладинка") with { CoverMediaFileId = 999_999 });
        Assert.Equal("coverMediaFileId", Assert.Single(unknown.ValidationErrors).Identifier);

        var fileId = _media.Add("https://cdn.example.test/articles/new.webp");
        var replaced = await SendUpdate(provider, created.Value.Id,
            Input(created.Value.Slug, "Стара обкладинка") with { CoverMediaFileId = fileId });
        Assert.Equal(new ArticleCoverView(fileId, "https://cdn.example.test/articles/new.webp", null), replaced.Value.Cover);
        await using (var db = postgres.CreateContext())
            Assert.False(await db.ArticleCovers.AnyAsync(c => c.ArticleId == ArticleId.Create(created.Value.Id)));

        var kept = await SendUpdate(provider, created.Value.Id, Input(created.Value.Slug, "Інший заголовок"));
        Assert.Equal(fileId, kept.Value.Cover.MediaFileId);

        var removed = await SendUpdate(provider, created.Value.Id,
            Input(created.Value.Slug, "Інший заголовок") with { RemoveCover = true });
        Assert.Equal(ArticleCoverView.None, removed.Value.Cover);
    }

    [SkippableFact]
    public async Task MoveLegacyCovers_UploadsBytesBySlug_LinksTheFile_AndASecondRunMovesNothing()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        await using var provider = BuildProvider();
        List<ArticleCover> seeded;
        await using (var db = postgres.CreateContext())
            seeded = await db.ArticleCovers.AsNoTracking().ToListAsync();

        try
        {
            var first = await SendMoveLegacy(provider);
            var second = await SendMoveLegacy(provider);

            Assert.Equal(seeded.Count, first.Value.Moved);
            Assert.Equal(0, second.Value.Moved);
            var upload = _media.Uploads.First();
            Assert.Equal(("articles", "Статті"), (upload.StorageDirectory, upload.FolderName));
            Assert.EndsWith(".webp", upload.FileName, StringComparison.Ordinal);
            await using var db = postgres.CreateContext();
            Assert.False(await db.ArticleCovers.AnyAsync());
            Assert.All((await db.Articles.AsNoTracking().ToListAsync()).Where(a => a.Id.Value <= 6), article =>
            {
                Assert.Null(article.CoverVersion);
                Assert.NotNull(article.CoverMediaFileId);
            });
        }
        finally
        {
            await using var db = postgres.CreateContext();
            await db.Database.ExecuteSqlRawAsync(
                """UPDATE content."Articles" SET "CoverVersion" = 1, "CoverMediaFileId" = NULL WHERE "Id" <= 6""");
            foreach (var cover in seeded)
                await db.Database.ExecuteSqlRawAsync(
                    """INSERT INTO content."ArticleCovers" ("ArticleId", "Data", "ContentType") VALUES ({0}, {1}, {2}) ON CONFLICT DO NOTHING""",
                    cover.ArticleId.Value, cover.Data, cover.ContentType);
        }
    }

    private async Task<Result<LegacyArticleCoversMoved>> SendMoveLegacy(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new MoveLegacyArticleCoversHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                scope.ServiceProvider.GetRequiredService<IContentRepository<ArticleCover>>(),
                _media,
                new TestContentEditor(),
                TimeProvider.System)
            .Handle(new MoveLegacyArticleCoversCommand(), default);
    }

    private static ArticleInput Input(string? slug, string ukTitle, bool isPublished = true) => new(slug, Day, isPublished,
    [
        new ArticleTextInput("uk", ukTitle, "Коротко.", "<p>Текст</p><script>x</script>"),
        new ArticleTextInput("ru", ukTitle, "Коротко.", "<p>Текст</p>"),
    ]);

    private async Task<Result<AdminArticleDetails>> SendCreate(ServiceProvider provider, ArticleInput input)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new CreateArticleHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                scope.ServiceProvider.GetRequiredService<IContentRepository<ArticleCover>>(),
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(),
                _media,
                scope.ServiceProvider.GetRequiredService<IArticleHtmlSanitizer>(),
                new TestContentEditor(),
                TimeProvider.System)
            .Handle(new CreateArticleCommand(input), default);
    }

    private async Task<Result<AdminArticleDetails>> SendUpdate(ServiceProvider provider, long id, ArticleInput input)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new UpdateArticleHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                scope.ServiceProvider.GetRequiredService<IContentRepository<ArticleCover>>(),
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(),
                _media,
                scope.ServiceProvider.GetRequiredService<IArticleHtmlSanitizer>(),
                new TestContentEditor(),
                TimeProvider.System)
            .Handle(new UpdateArticleCommand(id, input), default);
    }

    private async Task ResetAsync()
    {
        await using var db = postgres.CreateContext();
        await db.Database.ExecuteSqlRawAsync("""DELETE FROM content."Articles" WHERE "Id" > 6""");
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        return new ServiceCollection().AddContentInfrastructure(configuration).BuildServiceProvider();
    }
}
