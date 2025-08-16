namespace Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILoggingService loggingService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IUseCase
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var requestName = typeof(TRequest).Name;
        loggingService.LogValidationStarted(requestName);

        var context   = new ValidationContext<TRequest>(request);
        var results  = await Task.WhenAll(validators.Select(
            v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors)
            .Where(f => f is not null).ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var message = string.Join("; ", failures.Select(f => f.ErrorMessage));
        var details = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        loggingService.LogValidationFailed(requestName, message, details);

        // FluentValidation -> Ardalis.Result.ValidationError
        var fv     = new FluentValidation.Results.ValidationResult(failures);
        var errors = fv.AsErrors();

        var t = typeof(TResponse);

        // 1) Негeneric Result
        if (t == typeof(Result))
            return (TResponse)(object)Result.Invalid(errors);

        // 2) Generic Result<T>
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = t.GetGenericArguments()[0];
            var closed    = typeof(Result<>).MakeGenericType(innerType);
            var invalid   = closed.GetMethod("Invalid", [typeof(IEnumerable<ValidationError>)])!;
            var boxed     = invalid.Invoke(null, [errors])!;
            return (TResponse)boxed;
        }

        throw new ValidationException(failures);
    }

}
