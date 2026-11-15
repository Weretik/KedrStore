using Ardalis.Result;
using BuildingBlocks.Application.Results;
using Content.Application.Abstractions.Persistence;
using Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;
using Content.Application.Features.SeoSettings.Shared;
using Content.Application.Features.SiteSeo.Commands.UpdatePageSeo;
using Content.Application.Features.SiteSeo.Shared;
using Content.Domain.Seo;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Content;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content.Seo;

/// <summary>Database rules 4: an SEO form that read an older version cannot overwrite a newer save.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class SeoFormVersionPostgresTests(ContentPostgresFixture postgres)
{
    private readonly FakeArticleMediaLibrary _media = new();

    [SkippableFact]
    public async Task PageSeo_FirstSaveOfAnUnsavedPage_ThenAnOlderVersion_AnswersConflict()
    {
        postgres.SkipIfUnavailable();
        await using (var db = postgres.CreateContext())
            await db.PageSeo.Where(seo => seo.Page == SitePages.Wholesale).ExecuteDeleteAsync();
        await using var provider = BuildProvider();

        var first = await SavePage(provider, "Опт", 0);
        var stale = await SavePage(provider, "Ще раз з порожньої форми", 0);
        var second = await SavePage(provider, "Друга правка", first.Value.Version);
        var olderThanSecond = await SavePage(provider, "Стара форма", first.Value.Version);

        Assert.True(first.IsSuccess);
        Assert.NotEqual(0u, first.Value.Version);
        Assert.Equal(ResultStatus.Conflict, stale.Status);
        Assert.True(second.IsSuccess);
        Assert.Equal(ResultStatus.Conflict, olderThanSecond.Status);
        Assert.Equal(RowVersions.ChangedMessage, Assert.Single(olderThanSecond.Errors));
    }

    [SkippableFact]
    public async Task SiteSettings_WithAnOlderVersion_AnswersConflict_AndWithoutAVersionSaves()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        try
        {
            var first = await SaveSettings(provider, "G-FIRST", null);
            var second = await SaveSettings(provider, "G-SECOND", first.Value.Version);
            var stale = await SaveSettings(provider, "G-STALE", first.Value.Version);

            Assert.True(first.IsSuccess);
            Assert.NotEqual(first.Value.Version, second.Value.Version);
            Assert.Equal(ResultStatus.Conflict, stale.Status);
            await using var db = postgres.CreateContext();
            Assert.Equal("G-SECOND", (await db.SiteSeoSettings.AsNoTracking().SingleAsync()).GoogleAnalyticsId);
        }
        finally
        {
            await SaveSettings(provider, null, null);
        }
    }

    private async Task<Result<AdminPageSeo>> SavePage(ServiceProvider provider, string title, uint? version)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new UpdatePageSeoHandler(scope.ServiceProvider.GetRequiredService<IContentRepository<PageSeo>>(),
                new TestContentEditor(), TimeProvider.System)
            .Handle(new UpdatePageSeoCommand(SitePages.Wholesale, title, null, null, null, version), default);
    }

    private async Task<Result<SiteSeoSettingsDetails>> SaveSettings(ServiceProvider provider, string? analyticsId,
        uint? version)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new UpdateSiteSeoSettingsHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<SiteSeoSettings>>(), _media,
                new TestContentEditor(), TimeProvider.System)
            .Handle(new UpdateSiteSeoSettingsCommand(
                new SiteSeoSettingsInput(analyticsId, null, null, null, null, version)), default);
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        return new ServiceCollection().AddContentInfrastructure(configuration).BuildServiceProvider();
    }
}
