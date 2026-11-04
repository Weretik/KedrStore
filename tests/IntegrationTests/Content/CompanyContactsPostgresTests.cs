using Content.Application.Contracts.Persistence;
using Content.Application.Features.Contacts.Common;
using Content.Application.Features.Contacts.Get;
using Content.Application.Features.Contacts.Update;
using Content.Domain.Contacts;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>The contacts migration seeds the values the site used, and the one row can only be updated.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class CompanyContactsPostgresTests(ContentPostgresFixture postgres)
{
    private static readonly CompanyContactsInput Original = new("+380687135049", "kedr.class19@gmail.com",
        "https://t.me/+380687135049", "+380687135049", "https://maps.app.goo.gl/D2rH3HcaxLfxik5H6", "Харків", "Харьков");

    [SkippableFact]
    public async Task Migration_SeedsTheSiteContacts()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await new GetCompanyContactsQueryHandler(
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>())
            .Handle(new GetCompanyContactsQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("+380687135049", result.Value.SalesPhone);
        Assert.Equal("Харків", result.Value.CityUk);
    }

    [SkippableFact]
    public async Task Update_PersistsAndASecondRowIsRejected()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        try
        {
            await using (var scope = provider.CreateAsyncScope())
            {
                var result = await new UpdateCompanyContactsCommandHandler(
                        scope.ServiceProvider.GetRequiredService<IContentRepository<CompanyContacts>>(),
                        TimeProvider.System)
                    .Handle(new UpdateCompanyContactsCommand(Original with { Email = "sales@classkedr.com.ua" }), default);
                Assert.True(result.IsSuccess);
            }

            await using var db = postgres.CreateContext();
            Assert.Equal("sales@classkedr.com.ua", (await db.CompanyContacts.AsNoTracking().SingleAsync()).Email);
            await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO content."CompanyContacts"
                ("Id","SalesPhone","Email","TelegramUrl","ViberPhone","MapUrl","CityUk","CityRu","UpdatedAt")
                VALUES (2,'+380','a@b.c','https://t.me/a','+380','https://m','x','x',now())
                """));
        }
        finally
        {
            await using var scope = provider.CreateAsyncScope();
            await new UpdateCompanyContactsCommandHandler(
                    scope.ServiceProvider.GetRequiredService<IContentRepository<CompanyContacts>>(),
                    TimeProvider.System)
                .Handle(new UpdateCompanyContactsCommand(Original), default);
        }
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        return new ServiceCollection().AddContentInfrastructureServices(configuration).BuildServiceProvider();
    }
}
