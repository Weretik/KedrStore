using System.Security.Claims;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Work.Application.Features.Activity.Commands.RecordWorkActivity;

namespace Work.Api.Features.Activity;

/// <summary>
/// Global result filter for spec work/001, R-5: after a staff member's successful change through an <c>Admin*</c>
/// controller of any module, writes one activity entry. No other module knows about it, and a failure to write
/// is logged and never changes the response.
/// </summary>
public sealed class WorkActivityFilter(ILogger<WorkActivityFilter> logger) : IAsyncResultFilter
{
    private static readonly HashSet<string> SkippedRouteValues = ["controller", "action"];

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var http = context.HttpContext;
        if (!IsChange(http.Request.Method) || http.Response.StatusCode is < 200 or > 299)
            return;
        if (context.ActionDescriptor is not ControllerActionDescriptor action || !IsLoggedController(action))
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
            AreaOf(action.ControllerName),
            action.ActionName,
            http.Request.Method,
            TargetOf(context.RouteData.Values));
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

    internal static bool IsChange(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    internal static bool IsLoggedController(ControllerActionDescriptor action) =>
        action.ControllerName.StartsWith("Admin", StringComparison.Ordinal)
        && action.ControllerTypeInfo.Assembly != typeof(WorkActivityFilter).Assembly;

    /// <summary><c>AdminCategories</c> → <c>Categories</c>.</summary>
    internal static string AreaOf(string controllerName) =>
        controllerName.Length > "Admin".Length ? controllerName["Admin".Length..] : controllerName;

    /// <summary>Route values such as <c>id=12, lang=uk</c>, in name order.</summary>
    internal static string TargetOf(IEnumerable<KeyValuePair<string, object?>> routeValues) =>
        string.Join(", ", routeValues
            .Where(pair => !SkippedRouteValues.Contains(pair.Key) && pair.Value is not null)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}={pair.Value}"));
}
