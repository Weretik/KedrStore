namespace BuildingBlocks.Domain.Errors;

public interface IDomainError
{
    string Code { get; }
    string Message { get; }
    object? Details { get; }
}
