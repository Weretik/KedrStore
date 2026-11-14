using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Api.Results;

/// <summary>
/// The one place that turns a use-case result into a controller answer. Success keeps its status and body (200 with
/// the value, or empty for a result without a value; 201 with the value; 204); every failure is
/// <c>application/problem+json</c> built by <see cref="ResultProblems"/>. Removed when the last module moves to
/// Minimal API (platform spec 005).
/// </summary>
public static class ResultToActionResult
{
    public static ActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
        => Answer(controller, result, hasValue: true);

    public static ActionResult ToActionResult(this ControllerBase controller, Ardalis.Result.Result result)
        => Answer(controller, result, hasValue: false);

    public static ActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        => controller.ToActionResult(result);

    public static ActionResult ToActionResult(this Ardalis.Result.Result result, ControllerBase controller)
        => controller.ToActionResult(result);

    /// <summary>The failure of any result as <c>application/problem+json</c>.</summary>
    public static ActionResult ToProblem(this ControllerBase controller, Ardalis.Result.IResult result)
        => ToProblem(ResultProblems.From(controller.HttpContext, result));

    public static ActionResult ToProblem(ProblemDetails problem) => new ObjectResult(problem)
    {
        StatusCode = problem.Status,
        ContentTypes = { ResultProblems.ContentType },
    };

    private static ActionResult Answer(ControllerBase controller, Ardalis.Result.IResult result, bool hasValue)
        => result.Status switch
        {
            ResultStatus.Ok => hasValue ? controller.Ok(result.GetValue()) : controller.Ok(),
            ResultStatus.Created => controller.StatusCode(StatusCodes.Status201Created, result.GetValue()),
            ResultStatus.NoContent => controller.NoContent(),
            _ => controller.ToProblem(result),
        };
}
