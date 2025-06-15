using Application.Common.Validation;

namespace Application.Common.Abstractions.Validation
{
    public interface IValidator<in T>
    {
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
    }
}
