using BuildingBlocks.Application.Logging;
using BuildingBlocks.Application.Logging.Failures;
using BuildingBlocks.Application.Results;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Behaviors;

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
            var messageType = typeof(TMessage).Name;
            CancellationLog.Cancelled(logger, messageType);

            throw;
        }
        catch (DbUpdateConcurrencyException) when (IsResult())
        {
            // Another save changed the row between this command's read and its write (database rules 4).
            return Failure(nameof(Result.Conflict), new[] { RowVersions.ChangedMessage });
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

            if (IsResult())
                return Failure(nameof(Result.Error), "Виникла невідома помилка.");

            throw;
        }
    }

    private static bool IsResult()
        => typeof(TResponse) == typeof(Result)
           || (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>));

    /// <summary>
    /// A failed <c>Result</c> or <c>Result&lt;T&gt;</c> from its static factory <paramref name="factory"/>
    /// (<c>Error(string)</c> or <c>Conflict(params string[])</c>).
    /// </summary>
    private static TResponse Failure(string factory, object argument)
    {
        var method = typeof(TResponse).GetMethod(factory, [argument.GetType()]);
        ArgumentNullException.ThrowIfNull(method);
        return (TResponse)method.Invoke(null, [argument])!;
    }

    private static bool IsCritical(Exception ex)
        => ex is OutOfMemoryException or AccessViolationException or ThreadAbortException;
}
