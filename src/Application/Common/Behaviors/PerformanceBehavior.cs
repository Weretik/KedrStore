namespace Application.Common.Behaviors;

public class PerformanceBehavior<TMessage, TResponse>(
    ILogger<PerformanceBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private const int ThresholdMs = 500;

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            return await next(message, cancellationToken);
        }
        finally
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds >= ThresholdMs)
            {
                PerformanceLog.Slow(
                    logger,
                    typeof(TMessage).Name,
                    sw.ElapsedMilliseconds,
                    ThresholdMs);
            }
        }
    }
}
