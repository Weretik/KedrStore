using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Api.Results;

/// <summary>
/// Builds the RFC 9457 body of a failed result: <c>detail</c> is the first message (Ukrainian), <c>code</c> the
/// first validator error code or the status code word, <c>errors</c> the field messages of a 400, and
/// <c>traceId</c> the request's trace identifier.
/// </summary>
public static class ResultProblems
{
    public const string ContentType = "application/problem+json";

    private const string UnexpectedMessage = "Виникла невідома помилка.";

    public static int StatusOf(ResultStatus status) => status switch
    {
        ResultStatus.Invalid => StatusCodes.Status400BadRequest,
        ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
        ResultStatus.NotFound => StatusCodes.Status404NotFound,
        ResultStatus.Conflict => StatusCodes.Status409Conflict,
        ResultStatus.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError,
    };

    public static ProblemDetails From(HttpContext httpContext, Ardalis.Result.IResult result)
    {
        var status = StatusOf(result.Status);
        if (result.Status == ResultStatus.Invalid)
            return Invalid(httpContext, result.ValidationErrors.ToList());

        var detail = status >= StatusCodes.Status500InternalServerError && status != StatusCodes.Status503ServiceUnavailable
            ? FirstMessage(result.Errors) ?? UnexpectedMessage
            : FirstMessage(result.Errors);
        return Create(httpContext, status, ProblemCodes.ForStatus(status), detail);
    }

    public static ProblemDetails Create(HttpContext httpContext, int status, string code, string? detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = TitleOf(status),
            Type = TypeOf(status),
            Detail = detail,
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }

    private static ProblemDetails Invalid(HttpContext httpContext, IReadOnlyList<ValidationError> errors)
    {
        var first = errors.FirstOrDefault();
        var problem = new ValidationProblemDetails(errors
            .GroupBy(error => error.Identifier ?? string.Empty, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray(),
                StringComparer.Ordinal))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = TitleOf(StatusCodes.Status400BadRequest),
            Type = TypeOf(StatusCodes.Status400BadRequest),
            Detail = first?.ErrorMessage,
        };
        problem.Extensions["code"] = OwnCode(first?.ErrorCode) ?? ProblemCodes.Validation;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }

    /// <summary>
    /// A code a rule chose itself. FluentValidation names its built-in checks <c>NotEmptyValidator</c> and the like;
    /// those say nothing to a client, so the answer falls back to <c>validation</c>.
    /// </summary>
    private static string? OwnCode(string? errorCode) =>
        string.IsNullOrWhiteSpace(errorCode) || errorCode.EndsWith("Validator", StringComparison.Ordinal)
            ? null
            : errorCode;

    private static string? FirstMessage(IEnumerable<string> messages) =>
        messages.FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));

    private static string TitleOf(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Некоректні дані.",
        StatusCodes.Status401Unauthorized => "Потрібно увійти.",
        StatusCodes.Status403Forbidden => "Недостатньо прав.",
        StatusCodes.Status404NotFound => "Не знайдено.",
        StatusCodes.Status409Conflict => "Дані вже змінилися.",
        StatusCodes.Status503ServiceUnavailable => "Сервіс тимчасово недоступний.",
        _ => UnexpectedMessage,
    };

    private static string TypeOf(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        StatusCodes.Status503ServiceUnavailable => "https://tools.ietf.org/html/rfc9110#section-15.6.4",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
    };
}
