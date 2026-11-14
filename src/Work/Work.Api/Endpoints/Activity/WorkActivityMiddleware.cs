using System.Reflection;
using System.Security.Claims;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Work.Application.Features.Activity.Commands.RecordWorkActivity;

namespace Work.Api.Endpoints.Activity;

/// <summary>
/// Spec work/001, R-5: after a staff member's successful change through an <c>Admin*</c> endpoint group (or, until
/// the last module moves to Minimal API, an <c>Admin*</c> controller) of any module, writes one activity entry. No
/// other module knows about it, and a failure to write is logged and never changes the response.
/// </summary>
public sealed class WorkActivityMiddleware(RequestDelegate next, ILogger<WorkActivityMiddleware> logger)
{
    private static readonly HashSet<string> SkippedRouteValues = ["controller", "action"];

    public async Task InvokeAsync(HttpContext http)
    {
        await next(http);

        if (!IsChange(http.Request.Method) || http.Response.StatusCode is < 200 or > 299)
            return;
        if (http.GetEndpoint() is not { } endpoint || Describe(endpoint) is not { } source || !IsLogged(source))
            return;
        if (!Guid.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return;
        // Staff only, by the same policy as the staff areas of the admin.
        var authorization = http.RequestServices.GetRequiredService<IAuthorizationService>();
        if (!(await authorization.AuthorizeAsync(http.User, PolicyNames.RequireManagerRole)).Succeeded)
            return;

        var command = new RecordWorkActivityCommand(
            userId,
            http.User.FindFirstValue(ClaimTypes.Email) ?? http.User.Identity?.Name,
            AreaOf(source.Group),
            source.Operation,
            http.Request.Method,
            TargetOf(http.Request.RouteValues));
        try
        {
            var sender = http.RequestServices.GetRequiredService<ISender>();
            var result = await sender.Send(command, CancellationToken.None);
            if (!result.IsSuccess)
                logger.LogWarning("Work activity was not recorded for {Area}.{Operation}: {Status}",
                    command.Area, command.Operation, result.Status);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Work activity was not recorded for {Area}.{Operation}",
                command.Area, command.Operation);
        }
    }

    /// <summary>The group (tag or controller name), the operation (endpoint or action name) and its assembly.</summary>
    internal sealed record Source(string Group, string Operation, Assembly Assembly);

    internal static Source? Describe(Endpoint endpoint)
    {
        if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is { } action)
            return new Source(action.ControllerName, action.ActionName, action.ControllerTypeInfo.Assembly);

        var tag = endpoint.Metadata.GetMetadata<ITagsMetadata>()?.Tags.FirstOrDefault();
        var name = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
        var handler = endpoint.Metadata.GetMetadata<MethodInfo>();
        return tag is null || name is null || handler?.DeclaringType is null
            ? null
            : new Source(tag, name, handler.DeclaringType.Assembly);
    }

    internal static bool IsLogged(Source source) =>
        source.Group.StartsWith("Admin", StringComparison.Ordinal)
        && source.Assembly != typeof(WorkActivityMiddleware).Assembly;

    internal static bool IsChange(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    /// <summary><c>AdminCategories</c> → <c>Categories</c>.</summary>
    internal static string AreaOf(string group) =>
        group.Length > "Admin".Length ? group["Admin".Length..] : group;

    /// <summary>Route values such as <c>id=12, lang=uk</c>, in name order.</summary>
    internal static string TargetOf(IEnumerable<KeyValuePair<string, object?>> routeValues) =>
        string.Join(", ", routeValues
            .Where(pair => !SkippedRouteValues.Contains(pair.Key) && pair.Value is not null)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}={pair.Value}"));
}
