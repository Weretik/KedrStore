using BuildingBlocks.Application.Logging;

namespace BuildingBlocks.Application.Behaviors;

public sealed class RequestLoggingBehavior<TMessage, TResponse>(
    ILogger<RequestLoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TMessage).Name;
        var timer = Stopwatch.StartNew();

        RequestLog.Handling(logger, requestName);

        try
        {
            ArgumentNullException.ThrowIfNull(next);
            var response = await next(message, cancellationToken);
            timer.Stop();

            RequestLog.Handled(logger, requestName, timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            RequestLog.Failed(logger, requestName, timer.ElapsedMilliseconds, ex);
            throw;
        }
    }
}
