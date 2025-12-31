using Catalog.Domain.Errors;

namespace BuildingBlocks.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public DomainError Error { get; }

    public DomainException(DomainError error) : base(error.Message)
    {
        Error = error;
    }
}
