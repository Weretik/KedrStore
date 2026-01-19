namespace BuildingBlocks.Domain.Abstractions;

public interface IDomainError
{
    string Code { get; }
    string Message { get; }
}
