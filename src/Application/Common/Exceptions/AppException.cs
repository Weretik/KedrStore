namespace Application.Common.Exceptions;

public sealed class AppException(string code, string description,  Exception? inner = null)
    : Exception(description, inner)
{
    public string Code { get; } = code;
    public string Description { get; } = description;

    public override string ToString()
        => $"{Code}: {Description}" + (InnerException != null ? $" | Inner: {InnerException.Message}" : "");
}
