using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
            return controller.ToActionResult(result);

        var etag = new EntityTagHeaderValue($"\"{version(result.Value)}\"");
        controller.Response.Headers.ETag = etag.ToString();
        controller.Response.Headers.CacheControl = "private, no-cache";

        var held = controller.Request.GetTypedHeaders().IfNoneMatch;
        if (held.Any(tag => tag.Equals(EntityTagHeaderValue.Any) || tag.Compare(etag, useStrongComparison: true)))
            return new StatusCodeResult(StatusCodes.Status304NotModified);

        return controller.Ok(result.Value);
    }
}
