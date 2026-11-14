using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace BuildingBlocks.Api.Results;

/// <summary>
/// The one place that turns a use-case result into a Minimal API answer. Success keeps the status and body the
/// controllers gave (200 with the value, or empty for a result without a value; 201 with the value; 204); every
/// failure is <c>application/problem+json</c> built by <see cref="ResultProblems"/>.
/// </summary>
public static class ResultHttp
{
    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext httpContext)
        => Answer(result, httpContext, hasValue: true);

    public static IResult ToHttpResult(this Ardalis.Result.Result result, HttpContext httpContext)
        => Answer(result, httpContext, hasValue: false);

    /// <summary>The failure of any result as <c>application/problem+json</c>.</summary>
    public static IResult ToProblem(this Ardalis.Result.IResult result, HttpContext httpContext)
        => TypedResults.Problem(ResultProblems.From(httpContext, result));

    private static IResult Answer(Ardalis.Result.IResult result, HttpContext httpContext, bool hasValue)
        => result.Status switch
        {
            ResultStatus.Ok => hasValue ? TypedResults.Ok(result.GetValue()) : TypedResults.Ok(),
            ResultStatus.Created => TypedResults.Json(result.GetValue(), statusCode: StatusCodes.Status201Created),
            ResultStatus.NoContent => TypedResults.NoContent(),
            _ => result.ToProblem(httpContext),
        };
}
