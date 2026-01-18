using BuildingBlocks.Domain.Errors;

namespace Catalog.Domain.Errors;

public sealed record DomainError(string Code, string Message, object? Details = null) : IDomainError
{
}
