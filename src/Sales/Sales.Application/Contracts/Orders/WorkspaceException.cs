namespace Sales.Application.Contracts.Orders;

public sealed class WorkspaceException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}
