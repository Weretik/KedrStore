namespace Application.Common.Behaviors;

public sealed class ResultExceptionGenericBehavior<TMessage, TResponse>(
    ILogger<ResultExceptionGenericBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, Result<TResponse>>
    where TMessage : IMessage
{
    public async ValueTask<Result<TResponse>> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, Result<TResponse>> next,
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
        catch (Exception ex)
        {
            var messageType = typeof(TMessage).Name;
            var exType = ex.GetType().Name;
            UnhandledLog.Error(logger, messageType, exType, ex);

            var msg = $"Сталася помилка {ex.Message}";
            return Result<TResponse>.Error(msg);
        }
    }
    private static bool IsCritical(Exception ex)
        => ex is OutOfMemoryException
           || ex is AccessViolationException
           || ex is AppDomainUnloadedException
           || ex is ThreadAbortException
           || ex is StackOverflowException;
}
