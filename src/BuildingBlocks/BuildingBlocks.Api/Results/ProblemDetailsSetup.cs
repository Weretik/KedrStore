using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Api.Results;

/// <summary>
/// ProblemDetails for the errors that do not come from a result: model-binding failures (400), empty 401/403/404
/// answers filled by status code pages, and unhandled exceptions. Each gets a <c>code</c> by status and the
/// request's <c>traceId</c> unless it already has them.
/// </summary>
public static class ProblemDetailsSetup
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services) =>
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var extensions = context.ProblemDetails.Extensions;
            var status = context.ProblemDetails.Status ?? context.HttpContext.Response.StatusCode;
            extensions.TryAdd("code", ProblemCodes.ForStatus(status));
            extensions["traceId"] = context.HttpContext.TraceIdentifier;
        });
}
