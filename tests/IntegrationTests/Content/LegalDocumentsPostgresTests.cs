using System.Security.Claims;
using BuildingBlocks.Application.Users;
using Content.Application.Abstractions.Integrations;
using Content.Application.Abstractions.Persistence;
using Content.Application.Features.Documents.Commands.CreateLegalDocumentDraft;
using Content.Application.Features.Documents.Commands.PublishLegalDocumentDraft;
using Content.Application.Features.Documents.Queries.GetPublishedLegalDocument;
using Content.Application.Features.Documents.Shared;
using Content.Domain.Documents;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>The documents migration seeds the four site texts; publishing swaps editions under the unique indexes.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class LegalDocumentsPostgresTests(ContentPostgresFixture postgres)
{
    [SkippableTheory]
    [InlineData(LegalDocumentKind.PublicOffer, "uk", "Публічна оферта")]
    [InlineData(LegalDocumentKind.PrivacyPolicy, "ru", "{email}")]
    [InlineData(LegalDocumentKind.ReturnsPolicy, "uk", "<h3>")]
    [InlineData(LegalDocumentKind.DeliveryPayment, "uk", "<ol>")]
    public async Task Migration_SeedsEditionOneOfEachDocument(LegalDocumentKind kind, string language, string expected)
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await new GetPublishedLegalDocumentHandler(
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>())
            .Handle(new GetPublishedLegalDocumentQuery(LegalDocumentKinds.ToSlug(kind), language), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 11, 3), result.Value.EffectiveFrom);
        Assert.Contains(expected, result.Value.Title + result.Value.BodyHtml);
    }

    [SkippableFact]
    public async Task Publish_ArchivesThePreviousEdition_AndASecondDraftIsRejectedByTheIndex()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        long? draftId = null;
        try
        {
            await using (var scope = provider.CreateAsyncScope())
            {
                var draft = await new CreateLegalDocumentDraftHandler(
                        scope.ServiceProvider.GetRequiredService<IContentRepository<LegalDocumentVersion>>(),
                        scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(),
                        scope.ServiceProvider.GetRequiredService<IArticleHtmlSanitizer>(), new AdminUser(),
                        TimeProvider.System)
                    .Handle(new CreateLegalDocumentDraftCommand("returns-policy", null), default);
                Assert.True(draft.IsSuccess);
                draftId = draft.Value.Id;
            }

            await using (var db = postgres.CreateContext())
            {
                await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(
                    """
                    INSERT INTO content."LegalDocumentVersions" ("Kind","Status","UpdatedAt","UpdatedBy")
                    VALUES ('ReturnsPolicy','Draft',now(),'x')
                    """));
            }

            await using (var scope = provider.CreateAsyncScope())
            {
                var published = await new PublishLegalDocumentDraftHandler(
                        scope.ServiceProvider.GetRequiredService<IContentRepository<LegalDocumentVersion>>(),
                        new AdminUser(), TimeProvider.System)
                    .Handle(new PublishLegalDocumentDraftCommand(draftId.Value, new DateOnly(2026, 11, 5)), default);
                Assert.True(published.IsSuccess);
                Assert.Equal(2, published.Value.Number);
                Assert.Equal(2, published.Value.Texts.Count);
            }

            await using (var db = postgres.CreateContext())
            {
                var statuses = await db.LegalDocumentVersions.AsNoTracking()
                    .Where(v => v.Kind == LegalDocumentKind.ReturnsPolicy).OrderBy(v => v.Number)
                    .Select(v => v.Status).ToListAsync();
                Assert.Equal([LegalDocumentStatus.Archived, LegalDocumentStatus.Published], statuses);
            }
        }
        finally
        {
            await using var db = postgres.CreateContext();
            await db.Database.ExecuteSqlRawAsync(
                "DELETE FROM content.\"LegalDocumentVersions\" WHERE \"Kind\" = 'ReturnsPolicy' AND \"UpdatedBy\" <> 'seed'");
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE content.\"LegalDocumentVersions\" SET \"Status\" = 'Published' WHERE \"Kind\" = 'ReturnsPolicy' AND \"Number\" = 1");
        }
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
