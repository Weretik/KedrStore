using BuildingBlocks.Application.Logging;

namespace BuildingBlocks.Application.Behaviors;

public class PerformanceBehavior<TMessage, TResponse>(
    ILogger<PerformanceBehavior<TMessage, TResponse>> logger,
    IOptions<PerformanceBehaviorOptions> options)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly PerformanceBehaviorOptions _options = options.Value;

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            ArgumentNullException.ThrowIfNull(next);
            return await next(message, cancellationToken);
        }
        finally
        {
            sw.Stop();
            var threshold = ResolveThresholdMs(typeof(TMessage), _options);
            if (sw.ElapsedMilliseconds >= threshold)
            {
                PerformanceLog.Slow(
                    logger,
                    typeof(TMessage).Name,
                    sw.ElapsedMilliseconds,
                    threshold);
            }
        }
    }

    private static int ResolveThresholdMs(Type messageType, PerformanceBehaviorOptions options)
    {
        var fullName = messageType.FullName;
        if (fullName is not null &&
            fullName.StartsWith("Identity.Application.Features.Auth.Session", StringComparison.Ordinal))
        {
            return options.AuthThresholdMs;
        }

        return options.DefaultThresholdMs;
    }
}
