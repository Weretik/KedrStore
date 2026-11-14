using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Abstractions.Errors;

namespace BuildingBlocks.Domain.Results;

/// <summary>
/// Runs several checks in order and keeps the first error, so an object with many fields is validated without a
/// branch per field: take each checked value, then return <see cref="Error"/> when it is set.
/// </summary>
public sealed class DomainChecks
{
    public DomainError? Error { get; private set; }

    public bool Failed => Error is not null;

    /// <summary>The value of a successful check; after a failure, the default value, which the caller discards.</summary>
    public T Take<T>(DomainResult<T> result)
    {
        if (result.IsSuccess)
            return result.Value;
        Error ??= result.Error;
        return default!;
    }

    public void Check(DomainResult result)
    {
        if (result.IsFailure)
            Error ??= result.Error;
    }
}
