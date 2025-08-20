namespace Application.Common.Behaviors;

public sealed class CriticalExceptionBehavior<TMessage, TResponse>(
    ILogger<CriticalExceptionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (IsCritical(ex))
        {
            var messageType = typeof(TMessage).Name;
            var exType = ex.GetType().Name;

            UnhandledLog.Critical(logger, messageType, exType, ex);
            throw;
        }
    }
    private static bool IsCritical(Exception ex)
        => ex is OutOfMemoryException
           || ex is AccessViolationException
           || ex is AppDomainUnloadedException
           || ex is ThreadAbortException
           || ex is StackOverflowException;
}
