namespace Application.Common.Behaviors;

public sealed class UnhandledExceptionBehavior<TMessage, TResponse>(
    ILogger<UnhandledExceptionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next(message, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var messageType = typeof(TMessage).Name;
            var exType = ex.GetType().Name;

            if (IsCritical(ex))
                UnhandledLog.Critical(logger, messageType, exType, ex);
            else
                UnhandledLog.Error(logger, messageType, exType, ex);

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
