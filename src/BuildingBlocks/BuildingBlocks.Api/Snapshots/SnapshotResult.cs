using Ardalis.Result;
using BuildingBlocks.Api.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace BuildingBlocks.Api.Snapshots;

/// <summary>
/// Answers a snapshot request with a strong <c>ETag</c>, or <c>304 Not Modified</c> with no body when the client
/// already holds that version (<c>If-None-Match</c>).
/// </summary>
public static class SnapshotResult
{
    public static ActionResult ToSnapshotResult<T>(this ControllerBase controller, Result<T> result,
        Func<T, string> version)
    {
        if (result.Status != ResultStatus.Ok)
            return controller.ToProblem(result);

        return IsHeld(controller.HttpContext, version(result.Value))
            ? new StatusCodeResult(StatusCodes.Status304NotModified)
            : controller.Ok(result.Value);
    }

    public static IResult ToSnapshotHttpResult<T>(this Result<T> result, HttpContext httpContext,
        Func<T, string> version)
    {
        if (result.Status != ResultStatus.Ok)
            return result.ToProblem(httpContext);

        return IsHeld(httpContext, version(result.Value))
            ? TypedResults.StatusCode(StatusCodes.Status304NotModified)
            : TypedResults.Ok(result.Value);
    }

    /// <summary>Sets the <c>ETag</c> and tells whether the client already holds this version.</summary>
    private static bool IsHeld(HttpContext httpContext, string version)
    {
        var etag = new EntityTagHeaderValue($"\"{version}\"");
        httpContext.Response.Headers.ETag = etag.ToString();
        httpContext.Response.Headers.CacheControl = "private, no-cache";

        var held = httpContext.Request.GetTypedHeaders().IfNoneMatch;
        return held.Any(tag => tag.Equals(EntityTagHeaderValue.Any) || tag.Compare(etag, useStrongComparison: true));
    }
}
