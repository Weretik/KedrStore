using BuildingBlocks.Application.Logging;

namespace BuildingBlocks.Application.Behaviors;

public class ValidationBehavior<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators,
    ILogger<ValidationBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage,TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            ArgumentNullException.ThrowIfNull(next);
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);
        var results  = await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var failuresList = results.SelectMany(result => result.Errors).Where(failure => failure is not null).ToList();

        if (failuresList.Count == 0)
        {
            ArgumentNullException.ThrowIfNull(next);
            return await next(message, cancellationToken);
        }

        ValidationLog.Failed(logger, typeof(TMessage).Name,
            failuresList.Select(failure => new
            {
                failure.PropertyName,
                failure.ErrorMessage
            }));

        var errorsList = new ValidationResult(failuresList).AsErrors();

        if (typeof(TResponse) == typeof(Result))
        {
            var result = Result.Invalid(errorsList);
            return (TResponse)(object)result;
        }
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = typeof(Result<>)
                .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                .GetMethod("Invalid", [typeof(List<ValidationError>)]);

            ArgumentNullException.ThrowIfNull(method);
            var result = method.Invoke(null, [errorsList]);
            return (TResponse)result!;
        }

        throw new ValidationException(failuresList);
    }
}
