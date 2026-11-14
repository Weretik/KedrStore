using Identity.Contracts.Authorization;
using Scheduling.Api.Endpoints.Tasks.GetScheduledTaskRuns;
using Scheduling.Api.Endpoints.Tasks.GetScheduledTasks;
using Scheduling.Api.Endpoints.Tasks.RunScheduledTaskNow;
using Scheduling.Api.Endpoints.Tasks.UpdateScheduledTaskSchedule;

namespace Scheduling.Api.Endpoints.Tasks;

/// <summary>«Адміністрування › Фонові задачі» (platform/007 R-11): list, history, schedule, «Запустити зараз»; Admin only.</summary>
public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapScheduledTasksEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("api/admin/scheduling/tasks")
            .WithTags("AdminScheduledTasks")
            .RequireAuthorization(PolicyNames.RequireAdminRole);
        GetScheduledTasksEndpoint.Map(group);
        GetScheduledTaskRunsEndpoint.Map(group);
        UpdateScheduledTaskScheduleEndpoint.Map(group);
        RunScheduledTaskNowEndpoint.Map(group);
        return endpoints;
    }

    /// <summary>Who did it, for the run history: the name, else the e-mail.</summary>
    internal static string Actor(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.Email) ?? "невідомий користувач";
}
