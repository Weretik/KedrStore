namespace Application.Common.Behaviors;

public sealed class ResultExceptionBehavior<TMessage>(
    ILogger<ResultExceptionBehavior<TMessage>> logger)
    : IPipelineBehavior<TMessage, Result>
    where TMessage : IMessage
{
    public async ValueTask<Result> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, Result> next,
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
            return Result.Error(msg);
        }
    }
    private static bool IsCritical(Exception ex)
        => ex is OutOfMemoryException
           || ex is AccessViolationException
           || ex is AppDomainUnloadedException
           || ex is ThreadAbortException
           || ex is StackOverflowException;
}
