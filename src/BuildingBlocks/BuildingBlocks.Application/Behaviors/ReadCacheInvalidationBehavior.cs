using BuildingBlocks.Application.Caching;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Drops cached reads after a successful command that declares <see cref="IInvalidatesReadCache"/> (ADR-0016). A failed
/// command saved nothing, so its cache stays.
/// </summary>
public sealed class ReadCacheInvalidationBehavior<TMessage, TResponse>(IReadCache cache)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        var response = await next(message, cancellationToken);
        if (message is IInvalidatesReadCache command && response is Ardalis.Result.IResult { Status: ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent })
        {
            foreach (var tag in command.InvalidatedCacheTags)
                await cache.InvalidateAsync(tag, CancellationToken.None);
        }

        return response;
    }
}
