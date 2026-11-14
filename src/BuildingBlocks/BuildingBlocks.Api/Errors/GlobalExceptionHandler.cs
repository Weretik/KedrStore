using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Api.Errors;

/// <summary>
/// The one place that turns an unhandled exception into an HTTP answer: it logs the exception and answers 500 as
/// ProblemDetails with the request's <c>traceId</c>, never the exception text or stack trace. Expected business
/// errors do not reach it; they travel as results and map to 400/404/409/403 in <c>ToHttpResult</c>.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        LogUnhandled(logger, httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier,
            exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Виникла невідома помилка.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path} (trace {TraceId}).")]
    private static partial void LogUnhandled(ILogger logger, string method, string path, string traceId,
        Exception exception);
}
