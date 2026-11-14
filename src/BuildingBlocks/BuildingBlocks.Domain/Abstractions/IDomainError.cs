namespace BuildingBlocks.Domain.Abstractions;

public interface IDomainError
{
    string Code { get; }
    string Message { get; }
    ErrorType Type { get; }

    /// <summary>The input field the error belongs to, when there is one; the admin shows the message under it.</summary>
    string? Field { get; }
}
