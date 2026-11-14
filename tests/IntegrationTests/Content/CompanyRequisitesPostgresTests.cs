using System.Security.Claims;
using BuildingBlocks.Application.Contracts;
using Content.Application.Abstractions.Persistence;
using Content.Application.Features.Requisites.Shared;
using Content.Application.Features.Requisites.Queries.GetCompanyRequisites;
using Content.Application.Features.Requisites.Queries.GetCompanyRequisitesHistory;
using Content.Application.Features.Requisites.Commands.UpdateCompanyRequisites;
using Content.Domain.Requisites;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>The requisites migration seeds the site values; an update and its log entry are saved together.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class CompanyRequisitesPostgresTests(ContentPostgresFixture postgres)
{
    private static readonly CompanyRequisitesInput Original = new("ФОП Овчинников Максим Володимирович",
        "2689400771", "24800170000032534", "UA303052990000026007025905216", "ПриватБанк", "305299");

    [SkippableFact]
    public async Task Migration_SeedsTheSiteRequisites()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await new GetCompanyRequisitesHandler(
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>())
            .Handle(new GetCompanyRequisitesQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("UA303052990000026007025905216", result.Value.Iban);
        Assert.Equal("ПриватБанк", result.Value.BankName);
    }

    [SkippableFact]
    public async Task Update_SavesTheLogEntryInJson_AndASecondRowIsRejected()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        long? entryId = null;
        try
        {
            await using (var scope = provider.CreateAsyncScope())
            {
                var result = await Handler(scope).Handle(
                    new UpdateCompanyRequisitesCommand(Original with { BankName = "АТ КБ «ПриватБанк»" }), default);
                Assert.True(result.IsSuccess);
            }

            await using (var scope = provider.CreateAsyncScope())
            {
                var history = await new GetCompanyRequisitesHistoryHandler(
                        scope.ServiceProvider.GetRequiredService<IReadContentDbContext>())
                    .Handle(new GetCompanyRequisitesHistoryQuery(), default);
                var entry = history.Value[0];
                entryId = entry.Id;
                Assert.Equal("admin@classkedr.com.ua", entry.ChangedByName);
                var change = Assert.Single(entry.Changes);
                Assert.Equal(("bankName", "ПриватБанк", "АТ КБ «ПриватБанк»"),
                    (change.Field, change.OldValue, change.NewValue));
            }

            await using var db = postgres.CreateContext();
            await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO content."CompanyRequisites"
                ("Id","LegalName","Code","RegistrationExtractNumber","Iban","BankName","BankCode","UpdatedAt")
                VALUES (2,'x','12345678','1','UA00','x','000000',now())
                """));
        }
        finally
        {
            await using (var scope = provider.CreateAsyncScope())
                await Handler(scope).Handle(new UpdateCompanyRequisitesCommand(Original), default);
            await using var db = postgres.CreateContext();
            await db.Database.ExecuteSqlRawAsync(
                "DELETE FROM content.\"CompanyRequisitesChanges\" WHERE \"Id\" >= {0}", entryId ?? 0);
        }
    }

    private static UpdateCompanyRequisitesHandler Handler(AsyncServiceScope scope)
    {
        return new UpdateCompanyRequisitesHandler(
            scope.ServiceProvider.GetRequiredService<IContentRepository<CompanyRequisites>>(),
            scope.ServiceProvider.GetRequiredService<IContentRepository<CompanyRequisitesChange>>(),
            new AdminUser(), TimeProvider.System);
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        return new ServiceCollection().AddContentInfrastructure(configuration).BuildServiceProvider();
    }

    private sealed class AdminUser : ICurrentUserService
    {
        public string UserId => "1";
        public bool IsAuthenticated => true;
        public string UserName => "admin";
        public string Email => "admin@classkedr.com.ua";
        public IEnumerable<string> Roles => ["Admin"];
        public IEnumerable<Claim> Claims => [];
        public bool IsInRole(string role) => role == "Admin";
        public bool HasPermission(string permission) => false;
        public string GetClaimValue(string claimType) => string.Empty;
    }
}
