using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Api.Result;

public static class ResultToActionResult
{
    public static ActionResult ToActionResult<T>(this ControllerBase c, Result<T> result)
        => result.Status switch
        {
            ResultStatus.Ok => c.Ok(result.Value),
            ResultStatus.NotFound => c.NotFound(),
            ResultStatus.Invalid => c.BadRequest(result.ValidationErrors),
            ResultStatus.Conflict => c.Conflict(result.Errors),
            ResultStatus.Forbidden => c.Forbid(),
            ResultStatus.Unauthorized => c.Unauthorized(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
}
