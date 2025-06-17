namespace Application.Common.Exceptions;

public sealed class AppException(
    string code,
    string description,
    string? technicalMessage = null,
    Exception? inner = null)
    : Exception(technicalMessage ?? description, inner)
{
    public string Code { get; } = code;
    public string Description { get; } = description;

    public static AppException From(AppError error)=>
        new (error.Code, error.Description, error.Details);
}
