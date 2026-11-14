namespace BuildingBlocks.Domain.Abstractions.Errors;

/// <summary>The kind of a business error; the shared Api maps it to one HTTP status.</summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unexpected,
}
