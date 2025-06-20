namespace Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase
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
            logger.LogWarning($"🐢 Long Running UseCase: {requestName} [{elapsedMs}ms] | Payload: {request}");
        }

        return response;
    }
}
