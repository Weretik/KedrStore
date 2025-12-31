namespace Catalog.Domain.Errors;

public sealed record DomainError(
    string Code,
    string Message,
    object? Meta = null
);
