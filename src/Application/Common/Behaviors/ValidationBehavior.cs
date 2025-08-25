namespace Application.Common.Behaviors;

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
        var results  = await Task.WhenAll(
            validators.Select(v=> v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count == 0)
        {
            ArgumentNullException.ThrowIfNull(next);
            return await next(message, cancellationToken);

        }

        ValidationLog.Failed(logger, typeof(TMessage).Name,
            failures.Select(f => new { f.PropertyName, f.ErrorMessage })
        );

        var errors = new FluentValidation.Results.ValidationResult(failures).AsErrors();

        if (typeof(TResponse) == typeof(Result))
        {
            var result = Result.Invalid(errors);
            return (TResponse)(object)result;
        }
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = typeof(Result<>)
                .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                .GetMethod("Invalid", [typeof(List<ValidationError>)]);

            ArgumentNullException.ThrowIfNull(method);


            var result = method.Invoke(null, [errors])!;
            return (TResponse)result!;
        }

        throw new ValidationException(failures);

    }

}
