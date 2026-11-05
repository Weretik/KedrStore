using Content.Application.Contracts.Persistence;
using Content.Application.Features.Wholesale.Admin.Get;
using Content.Application.Features.Wholesale.Admin.Update;
using Content.Application.Features.Wholesale.Common;
using Content.Application.Features.Wholesale.Public.Get;
using Content.Domain.Wholesale;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Content;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>The migration seeds the texts the site kept in code; both languages round-trip through jsonb.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class WholesalePagePostgresTests(ContentPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Migration_SeedsTheSiteTextsInBothLanguages()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IReadContentDbContext>();

        var uk = await new GetPublicWholesalePageQueryHandler(db).Handle(new GetPublicWholesalePageQuery("uk"), default);
        var ru = await new GetPublicWholesalePageQueryHandler(db).Handle(new GetPublicWholesalePageQuery("ru"), default);

        Assert.Equal("Дверна фурнітура оптом", uk.Value.Text.HeroTitle);
        Assert.Equal("Дверная фурнитура оптом", ru.Value.Text.HeroTitle);
        Assert.Equal(3, uk.Value.Text.Steps.Count);
        Assert.Equal("Як дізнатися оптові ціни та умови?", uk.Value.Text.Faq[0].Question);
        Assert.Equal("Обсудим вашу закупку", ru.Value.Text.ClosingTitle);
    }

    [SkippableFact]
    public async Task Update_ReplacesBothLanguages_AndASecondRowIsRejected()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        AdminWholesalePage original;
        await using (var scope = provider.CreateAsyncScope())
            original = (await new GetAdminWholesalePageQueryHandler(
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>())
                .Handle(new GetAdminWholesalePageQuery(), default)).Value;

        try
        {
            var changed = original.Texts.Select(text => text with
            {
                HeroTitle = text.HeroTitle + " і вроздріб",
                Faq = [.. text.Faq, new WholesaleFaqText("Нове питання?", "Нова відповідь.")],
            }).ToList();
            var saved = await SendUpdate(provider, changed);

            Assert.True(saved.IsSuccess);
            Assert.Equal(TestContentEditor.EmailAddress, saved.Value.UpdatedBy);
            await using var db = postgres.CreateContext();
            var stored = await db.WholesalePages.AsNoTracking().SingleAsync();
            Assert.EndsWith("і вроздріб", stored.Uk.HeroTitle, StringComparison.Ordinal);
            Assert.Equal(4, stored.Ru.Faq.Count);
            await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(
                """INSERT INTO content."WholesalePages" ("Id","Uk","Ru","UpdatedAt") VALUES (2,'{}','{}',now())"""));
        }
        finally
        {
            await SendUpdate(provider, original.Texts);
        }
    }

    private static async Task<Ardalis.Result.Result<AdminWholesalePage>> SendUpdate(ServiceProvider provider,
        IReadOnlyList<WholesaleTextDto> texts)
    {
        await using var scope = provider.CreateAsyncScope();
        return await new UpdateWholesalePageCommandHandler(
                scope.ServiceProvider.GetRequiredService<IContentRepository<WholesalePage>>(),
                new TestContentEditor(),
                TimeProvider.System)
            .Handle(new UpdateWholesalePageCommand(texts), default);
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        return new ServiceCollection().AddContentInfrastructureServices(configuration).BuildServiceProvider();
    }
}
