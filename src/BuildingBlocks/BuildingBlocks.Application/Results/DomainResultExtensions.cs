using BuildingBlocks.Domain.Results;
using ErrorType = BuildingBlocks.Domain.Abstractions.ErrorType;

namespace BuildingBlocks.Application.Results;

/// <summary>
/// Turns a domain outcome into the use-case result. The error kind picks the result status; the shared Api mapper
/// alone turns that status into HTTP.
/// </summary>
public static class DomainResultExtensions
{
    public static Result ToResult(this DomainError error) => error.Type switch
    {
        ErrorType.Validation => Result.Invalid(ToValidationError(error)),
        ErrorType.NotFound => Result.NotFound(error.Message),
        ErrorType.Conflict => Result.Conflict(error.Message),
        ErrorType.Forbidden => Result.Forbidden(error.Message),
        _ => Result.CriticalError(error.Message),
    };

    public static Result<T> ToResult<T>(this DomainError error) => error.Type switch
    {
        ErrorType.Validation => Result<T>.Invalid(ToValidationError(error)),
        ErrorType.NotFound => Result<T>.NotFound(error.Message),
        ErrorType.Conflict => Result<T>.Conflict(error.Message),
        ErrorType.Forbidden => Result<T>.Forbidden(error.Message),
        _ => Result<T>.CriticalError(error.Message),
    };

    public static Result ToResult(this DomainResult result)
        => result.IsSuccess ? Result.Success() : result.Error!.ToResult();

    /// <summary>The value on success, otherwise the failure as the matching result status.</summary>
    public static Result<T> AsResult<T>(this DomainResult<T> result)
        => result.IsSuccess ? Result.Success(result.Value) : result.Error!.ToResult<T>();

    /// <summary>The failure of <paramref name="result"/> as a result of another value type.</summary>
    public static Result<T> ToResult<T>(this DomainResult result)
        => result.IsFailure ? result.Error!.ToResult<T>() : throw new InvalidOperationException("The result succeeded.");

    public static Result<TOut> ToResult<TIn, TOut>(this DomainResult<TIn> result)
        => result.IsFailure ? result.Error!.ToResult<TOut>() : throw new InvalidOperationException("The result succeeded.");

    /// <summary>The validation error the frontend shows under <see cref="DomainError.Field"/>, or as a general error.</summary>
    public static ValidationError ToValidationError(this DomainError error)
        => new(error.Field ?? error.Code, error.Message, error.Code, ValidationSeverity.Error);
}
