namespace Application.Interfaces;

public interface ILoggingService
{
    void LogValidationStarted(string requestName);
    void LogValidationFailed(string requestName, string message, string details);
    void LogRequestStarted(string requestName, string? userId, object? payload);
    void LogRequestSucceeded(string requestName, long elapsedMs, string? userId, object? response);
    void LogRequestFailed(string requestName, long elapsedMs, string? userId, Exception exception);
    void LogAppException(string requestName, string code, string message);
    void LogBusinessRuleBroken(string requestName, string code, string message);
    void LogException(string requestName, Exception exception, string? source = null);
    void LogPerformance(string operation, long elapsedMs, params object[] args);

}
