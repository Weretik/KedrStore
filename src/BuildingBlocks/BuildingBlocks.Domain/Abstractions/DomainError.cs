namespace BuildingBlocks.Domain.Abstractions;

/// <summary>
/// A business outcome that is not success: a stable code, a message for the user in Ukrainian, its kind, and the
/// input field it belongs to, when there is one. Only the shared Api mapper turns <see cref="Type"/> into HTTP.
/// </summary>
public sealed record DomainError(string Code, string Message, ErrorType Type = ErrorType.Validation, string? Field = null)
    : IDomainError
{
    public static DomainError NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static DomainError Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static DomainError Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public DomainError OnField(string field) => this with { Field = field };
}
