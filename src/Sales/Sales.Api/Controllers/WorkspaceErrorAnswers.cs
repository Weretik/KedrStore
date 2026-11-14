using IResult = Ardalis.Result.IResult;

namespace Sales.Api.Controllers;

/// <summary>
/// The order workspace's error body <c>{ code, message }</c>, which the admin reads to word a problem. A 400 keeps the
/// code of the rule that failed; a 404 or 409 says only <c>not_found</c> or <c>conflict</c>.
/// </summary>
internal static class WorkspaceErrorAnswers
{
    public static ActionResult Error(this ControllerBase controller, IResult result) => result.Status switch
    {
        ResultStatus.Invalid => controller.BadRequest(FirstInvalid(result)),
        ResultStatus.NotFound => controller.NotFound(Body("not_found", result, "Не знайдено.")),
        ResultStatus.Conflict => controller.Conflict(Body("conflict", result, "Дані вже змінилися. Оновіть сторінку.")),
        _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
    };

    private static WorkspaceError FirstInvalid(IResult result)
    {
        var error = result.ValidationErrors.FirstOrDefault();
        return new WorkspaceError(
            string.IsNullOrEmpty(error?.ErrorCode) ? "invalid_request" : error.ErrorCode,
            error?.ErrorMessage ?? "Некоректні дані.");
    }

    private static WorkspaceError Body(string code, IResult result, string fallback) =>
        new(code, result.Errors.FirstOrDefault(message => !string.IsNullOrWhiteSpace(message)) ?? fallback);

    private sealed record WorkspaceError(string Code, string Message);
}
