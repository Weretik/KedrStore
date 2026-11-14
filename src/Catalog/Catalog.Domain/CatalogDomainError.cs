namespace Catalog.Domain;

public sealed record CatalogDomainError(string Code, string Message) : IDomainError
{
}
