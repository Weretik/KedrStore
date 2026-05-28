namespace Sales.Domain.Customers.Errors;

public sealed record DomainError(string Code, string Message) : IDomainError;
