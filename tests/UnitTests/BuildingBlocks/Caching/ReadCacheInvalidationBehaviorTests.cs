using Ardalis.Result;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Application.Caching;
using Mediator;

namespace UnitTests.BuildingBlocks.Caching;

public sealed class ReadCacheInvalidationBehaviorTests
{
    [Fact]
    public async Task SuccessfulCommand_DropsItsTags()
    {
        var cache = new RecordingReadCache();

        await new ReadCacheInvalidationBehavior<Save, Result>(cache)
            .Handle(new Save(), (_, _) => ValueTask.FromResult(Result.Success()), CancellationToken.None);

        Assert.Equal(["a", "b"], cache.Invalidated);
    }

    [Fact]
    public async Task FailedCommand_KeepsTheCache()
    {
        var cache = new RecordingReadCache();

        await new ReadCacheInvalidationBehavior<Save, Result>(cache)
            .Handle(new Save(), (_, _) => ValueTask.FromResult(Result.Invalid(new ValidationError("bad"))), CancellationToken.None);

        Assert.Empty(cache.Invalidated);
    }

    [Fact]
    public async Task CommandWithoutTags_KeepsTheCache()
    {
        var cache = new RecordingReadCache();

        await new ReadCacheInvalidationBehavior<Plain, Result>(cache)
            .Handle(new Plain(), (_, _) => ValueTask.FromResult(Result.Success()), CancellationToken.None);

        Assert.Empty(cache.Invalidated);
    }

    public sealed record Save : ICommand<Result>, IInvalidatesReadCache
    {
        public IReadOnlyCollection<string> InvalidatedCacheTags => ["a", "b"];
    }

    public sealed record Plain : ICommand<Result>;

    private sealed class RecordingReadCache : IReadCache
    {
        public List<string> Invalidated { get; } = [];

        public ValueTask<T> GetOrCreateAsync<T>(ReadCacheEntry entry, Func<CancellationToken, ValueTask<T>> read, CancellationToken cancellationToken)
            => read(cancellationToken);

        public ValueTask InvalidateAsync(string tag, CancellationToken cancellationToken)
        {
            Invalidated.Add(tag);
            return ValueTask.CompletedTask;
        }
    }
}
