namespace Infrastructure.Shared.Services;

public class LoggingService(ILogger<LoggingService> logger) : ILoggingService
{
    public void LogValidationStarted(string requestName)
    {
        logger.LogInformation("🧪 Validation started | Request {RequestType}",
            requestName);
    }

    public void LogValidationFailed(string requestName, string message, string details)
    {
        logger.LogWarning("❌ Validation error | Request: {RequestName} | Message: {Message} | Details: {Details}",
            requestName, message, details);
    }

    public void LogRequestStarted(string requestName, string? userId, object? payload)
    {
        logger.LogInformation("➡️ {RequestName} started | User: {UserId} | Payload: {@Payload}",
            requestName, userId, payload);
    }

    public void LogRequestSucceeded(string requestName, long elapsedMs, string? userId, object? response)
    {
        logger.LogInformation("✅ {RequestName} succeeded in {Elapsed}ms | User: {UserId} | Response: {@Response}",
            requestName, elapsedMs, userId, response);
    }

    public void LogRequestFailed(string requestName, long elapsedMs, string? userId, Exception exception)
    {
        logger.LogError(exception, "❌ {RequestName} failed in {Elapsed}ms | User: {UserId}",
            requestName, elapsedMs, userId);
    }

    public void LogAppException(string requestName, string code, string message)
    {
        logger.LogWarning("⚠️ AppException in {RequestName} | Code: {ErrorCode} | Message: {ErrorMessage}",
            requestName, code, message);
    }

    public void LogBusinessRuleBroken(string requestName, string code, string message)
    {
        logger.LogWarning("⚠️ BusinessRule broken in {RequestName} | Code: {ErrorCode} | Message: {ErrorMessage}",
            requestName, code, message);
    }

    public void LogException(string requestName, Exception exception, string? source = null)
    {
        logger.LogError(exception, "❌ Exception in {Source} | Request: {RequestName}",
            source ?? "UnknownSource", requestName);
    }

    public void LogPerformance(string operation, long elapsedMs, params object[] args)
    {
        logger.LogWarning("🐢 PERF: {Operation} [{ElapsedMs}ms] | {@Args}",
            operation, elapsedMs, args);
    }
}
