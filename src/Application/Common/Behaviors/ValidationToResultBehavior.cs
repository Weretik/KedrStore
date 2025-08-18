// Licensed to KedrStore Development Team under MIT License.

namespace Application.Common.Behaviors;

public sealed class ValidationToResultBehavior<TMessage>(
    IEnumerable<IValidator<TMessage>> validators,
    ILogger<ValidationToResultBehavior<TMessage>> logger)
    : IPipelineBehavior<TMessage, Result>
    where TMessage : IMessage
{
    public async ValueTask<Result> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, Result> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next(message, ct);

        var ctx      = new ValidationContext<TMessage>(message);
        var results  = await Task.WhenAll(validators.Select(v => v.ValidateAsync(ctx, ct)));
        var failures = results.SelectMany(r => r.Errors).Where(e => e is not null).ToList();

        if (failures.Count == 0)
            return await next(message, ct);

        // Логируем только имена полей и тексты — без значений (PII-safe)
        ValidationLog.Failed(
            logger,
            typeof(TMessage).Name,
            failures.Select(f => new { f.PropertyName, f.ErrorMessage })
        );

        var errors = new FluentValidation.Results.ValidationResult(failures).AsErrors();
        return Result.Invalid(errors);
    }
}
