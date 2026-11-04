using Ardalis.Result;
using Content.Application.Contracts.Images;
using Content.Application.Features.Articles.Admin.Create;
using Content.Application.Features.Articles.Admin.Delete;
using Content.Application.Features.Articles.Admin.Update;
using Content.Application.Features.Articles.Admin.UploadCover;
using Content.Application.Features.Articles.Common;
using Content.Application.Features.Articles.Public.GetBySlug;
using Content.Application.Features.Articles.Public.GetCover;
using Content.Application.Features.Articles.Public.GetList;
using Content.Application.Contracts.Html;
using Content.Application.Contracts.Persistence;
using Content.Domain.Articles;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.Identity.Avatars;
using IntegrationTests.TestSupport.Content;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>Migrations (schema, history table, legacy seed) and article handlers against real PostgreSQL.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class ArticlesPostgresTests(ContentPostgresFixture postgres)
{
    private static readonly DateOnly Day = new(2026, 11, 4);

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
        var list = await new GetPublicArticlesQueryHandler(db).Handle(new GetPublicArticlesQuery("ru"), default);
        var page = await new GetPublicArticleQueryHandler(db)
            .Handle(new GetPublicArticleQuery("uk", "prykhovani-petli"), default);
        var hidden = await new GetPublicArticleQueryHandler(db)
            .Handle(new GetPublicArticleQuery("uk", draft.Value.Slug), default);
        var cover = await new GetArticleCoverQueryHandler(db).Handle(new GetArticleCoverQuery(4), default);

        Assert.Equal(6, list.Value.Count);
        Assert.DoesNotContain(list.Value, item => item.Id == draft.Value.Id);
        Assert.StartsWith("<p>", page.Value.BodyHtml, StringComparison.Ordinal);
        Assert.Equal(ResultStatus.NotFound, hidden.Status);
        Assert.Equal("image/webp", cover.Value.ContentType);
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
        var update = await new UpdateArticleCommandHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(),
                scope.ServiceProvider.GetRequiredService<IArticleHtmlSanitizer>(),
                new TestContentEditor(),
                TimeProvider.System)
            .Handle(new UpdateArticleCommand(second.Value.Id, Input("prykhovani-petli", "Інша")), default);
        Assert.Equal(ResultStatus.Invalid, update.Status);
        Assert.Equal("slug", Assert.Single(update.ValidationErrors).Identifier);
    }

    [SkippableFact]
    public async Task Cover_UploadBumpsVersion_AndDeleteRemovesArticleWithCover()
    {
        postgres.SkipIfUnavailable();
        await ResetAsync();
        await using var provider = BuildProvider();
        var created = await SendCreate(provider, Input(null, "З обкладинкою"));
        var bytes = TestImages.Create(1200, 600);

        await using (var scope = provider.CreateAsyncScope())
        {
            var uploaded = await new UploadArticleCoverCommandHandler(
                    scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                    scope.ServiceProvider.GetRequiredService<IContentRepository<ArticleCover>>(),
                    scope.ServiceProvider.GetRequiredService<ICoverImageProcessor>(),
                    new TestContentEditor(),
                    TimeProvider.System)
                .Handle(new UploadArticleCoverCommand(created.Value.Id,
                    new CoverUpload("c.jpg", "image/jpeg", bytes.Length, () => new MemoryStream(bytes))), default);
            Assert.Equal(1, uploaded.Value.CoverVersion);
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var deleted = await new DeleteArticleCommandHandler(
                    scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                    scope.ServiceProvider.GetRequiredService<IContentRepository<ArticleCover>>())
                .Handle(new DeleteArticleCommand(created.Value.Id), default);
            Assert.True(deleted.IsSuccess);
        }

        await using var db = postgres.CreateContext();
        Assert.False(await db.Articles.AnyAsync(a => a.Id == ArticleId.Create(created.Value.Id)));
        Assert.False(await db.ArticleCovers.AnyAsync(c => c.ArticleId == ArticleId.Create(created.Value.Id)));
    }

    private static ArticleInput Input(string? slug, string ukTitle, bool isPublished = true) => new(slug, Day, isPublished,
    [
        new ArticleTextInput("uk", ukTitle, "Коротко.", "<p>Текст</p><script>x</script>"),
        new ArticleTextInput("ru", ukTitle, "Коротко.", "<p>Текст</p>"),
    ]);

    private static async Task<Result<AdminArticleDetails>> SendCreate(ServiceProvider provider, ArticleInput input)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new CreateArticleCommandHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<Article>>(),
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(),
                scope.ServiceProvider.GetRequiredService<IArticleHtmlSanitizer>(),
                new TestContentEditor(),
                TimeProvider.System)
            .Handle(new CreateArticleCommand(input), default);
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
        return new ServiceCollection().AddContentInfrastructureServices(configuration).BuildServiceProvider();
    }
}
