namespace Application.Common.Exceptions;

public sealed class AppException(string code, string message,  Exception? inner = null)
    : Exception(message, inner)
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    public override string ToString()
        => $"{Code}: {Message}" + (InnerException != null ? $" | Inner: {InnerException.Message}" : "");
}
