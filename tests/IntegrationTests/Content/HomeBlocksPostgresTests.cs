using Content.Application.Abstractions.Media;
using Content.Application.Abstractions.Persistence;
using Content.Application.Features.Home.Queries.GetPublicHomeBlocks;
using Content.Infrastructure.DependencyInjection;
using IntegrationTests.TestSupport.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Content;

/// <summary>The home page migration seeds the six site questions in order; brands start empty.</summary>
[Collection(ContentPostgresCollection.Name)]
public sealed class HomeBlocksPostgresTests(ContentPostgresFixture postgres)
{
    [SkippableFact]
    public async Task Migration_SeedsTheSixQuestionsInTheSiteOrder()
    {
        postgres.SkipIfUnavailable();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = postgres.ConnectionString })
            .Build();
        await using var provider = new ServiceCollection().AddContentInfrastructure(configuration)
            .BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var result = await new GetPublicHomeBlocksHandler(
                scope.ServiceProvider.GetRequiredService<IReadContentDbContext>(), new NoLogos())
            .Handle(new GetPublicHomeBlocksQuery("ru"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value.Faq.Count);
        Assert.Equal("Как выбрать замок для входных дверей?", result.Value.Faq[0].Question);
        Assert.Equal("Можно ли самостоятельно установить дверную фурнитуру?", result.Value.Faq[5].Question);
    }

    private sealed class NoLogos : IMediaImageReader
    {
        public Task<IReadOnlyDictionary<long, MediaImage>> GetImagesAsync(IReadOnlyCollection<long> mediaFileIds,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<long, MediaImage>>(new Dictionary<long, MediaImage>());
    }
}
