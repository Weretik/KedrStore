namespace Application.Common.Behaviors;

public sealed class ExceptionBehavior<TMessage, TResponse>(
    ILogger<ExceptionBehavior<TMessage, TResponse>> logger)
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
            ArgumentNullException.ThrowIfNull(next);
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
        catch (Exception ex)
        {
            var messageType = typeof(TMessage).Name;
            var exType = ex.GetType().Name;
            UnhandledLog.Error(logger, messageType, exType, ex);

            const string errorMessage = "Виникла невідома помилка.";

            if (typeof(TResponse) == typeof(Result))
            {
                var result = Result.Error(errorMessage);
                return (TResponse)(object)result;
            }
            if(typeof(TResponse).IsGenericType &&
               typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var method = typeof(Result<>)
                    .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                    .GetMethod("Error", [typeof(string)]);

                ArgumentNullException.ThrowIfNull(method);
                var result = method.Invoke(null, [errorMessage]);
                return (TResponse)result!;
            }

            throw;
        }
    }

    private static bool IsCritical(Exception ex)
        => ex is OutOfMemoryException or AccessViolationException or ThreadAbortException;
}
