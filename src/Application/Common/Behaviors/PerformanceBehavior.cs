namespace Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(
    ILoggingService loggingService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Порог в миллисекундах. Всё, что дольше — логгируем как медленное выполнение.
    private const int ThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        var elapsedMs = timer.ElapsedMilliseconds;

        if (elapsedMs > ThresholdMilliseconds)
        {
            loggingService.LogPerformance(requestName, elapsedMs, request);
        }

        return response;
    }
}
