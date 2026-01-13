using BuildingBlocks.Domain.Errors;

namespace BuildingBlocks.Domain.Exceptions;

public sealed class DomainException(IDomainError error) : Exception(error.Message)
{
    public IDomainError  Error { get; } = error;
}
