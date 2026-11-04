using Content.Application.Contracts.Gallery;
using Content.Application.Contracts.Persistence;
using Content.Application.Features.Gallery.Public.GetList;
using Content.Domain.Gallery;
using Content.Infrastructure.DataBase;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>The gallery migration seeds the seven site videos; the same video cannot be added twice.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class GalleryPostgresTests(ContentPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Migration_SeedsTheSevenVideosInTheSiteOrder()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await new GetPublicGalleryQueryHandler(
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(), new NoPhotos())
            .Handle(new GetPublicGalleryQuery("uk"), default);

        Assert.True(result.IsSuccess);
        var seeded = result.Value.Where(item => item.Kind == GalleryItemKind.Video).Take(7).ToList();
        Assert.Equal(["0FDRbSaQ9Ec", "j9wrjRub9xI", "1eB0xxFgwlw", "8hLIBALylkQ", "LQcVX5dC9MM", "SIWCZ5uVY_4", "ekr7ceJlJcg"],
            seeded.Select(item => item.YoutubeId));
        Assert.Equal("Прихована петля ZCG 40", seeded[0].Title);
    }

    [SkippableFact]
    public async Task TheSameVideoTwice_IsRejectedByTheIndex()
    {
        postgres.SkipIfUnavailable();
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        db.GalleryItems.Add(GalleryItem.CreateVideo("0FDRbSaQ9Ec", 100, GalleryItemTexts.Empty, DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        return new ServiceCollection().AddContentInfrastructureServices(configuration).BuildServiceProvider();
    }

    private sealed class NoPhotos : IGalleryImageReader
    {
        public Task<IReadOnlyDictionary<long, GalleryImage>> GetImagesAsync(IReadOnlyCollection<long> mediaFileIds,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<long, GalleryImage>>(new Dictionary<long, GalleryImage>());
    }
}
