using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Abstractions.Errors;

namespace BuildingBlocks.Domain.Results;

/// <summary>
/// The outcome of a domain operation: success, or the <see cref="DomainError"/> that a business rule reported.
/// Business rules return it; exceptions stay for programming errors only.
/// </summary>
public sealed class DomainResult
{
    private static readonly DomainResult Ok = new(null);

    private DomainResult(DomainError? error) => Error = error;

    /// <summary>The error of a failure; null on success.</summary>
    public DomainError? Error { get; }

    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    public static DomainResult Success() => Ok;
    public static DomainResult Failure(DomainError error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator DomainResult(DomainError error) => Failure(error);

    /// <summary>Success when the condition holds, otherwise the error.</summary>
    public static DomainResult Ensure(bool condition, Func<DomainError> error) => condition ? Ok : Failure(error());

    /// <summary>The first failure of the checks, in order, or success.</summary>
    public static DomainResult Combine(params DomainResult[] results)
        => results.FirstOrDefault(result => result.IsFailure) ?? Ok;

    /// <summary>For input the code built itself (fixed values, stored data), where a failure is a programming error.</summary>
    public void ThrowIfFailure()
    {
        if (Error is not null)
            throw new InvalidOperationException($"The operation failed with {Error.Code}: {Error.Message}");
    }
}

/// <summary>The outcome of a domain operation that produces a value.</summary>
public sealed class DomainResult<T>
{
    private readonly T? _value;

    private DomainResult(T? value, DomainError? error)
    {
        _value = value;
        Error = error;
    }

    public DomainError? Error { get; }

    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    /// <summary>The value of a success; reading it from a failure is a programming error.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"No value: the operation failed with {Error!.Code}.");

    public static DomainResult<T> Success(T value) => new(value, null);
    public static DomainResult<T> Failure(DomainError error) => new(default, error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator DomainResult<T>(T value) => Success(value);
    public static implicit operator DomainResult<T>(DomainError error) => Failure(error);
    public static implicit operator DomainResult(DomainResult<T> result) => result.WithoutValue();

    /// <summary>The plain outcome without the value.</summary>
    public DomainResult WithoutValue() => Error is null ? DomainResult.Success() : DomainResult.Failure(Error);

    /// <inheritdoc cref="DomainResult.ThrowIfFailure"/>
    public void ThrowIfFailure() => WithoutValue().ThrowIfFailure();
}
