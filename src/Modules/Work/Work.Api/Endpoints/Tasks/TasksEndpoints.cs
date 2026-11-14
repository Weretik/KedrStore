using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Work.Api.Endpoints.Tasks.GetMyWorkTasks;

using Work.Api.Endpoints.Tasks.DeleteMyWorkTask;
using Work.Api.Endpoints.Tasks.ChangeMyWorkTaskStatus;
using Work.Api.Endpoints.Tasks.CreateWorkTaskForUser;
using Work.Api.Endpoints.Tasks.SetMyWorkTaskStep;
using Work.Api.Endpoints.Tasks.CreateMyWorkTask;
using Work.Api.Endpoints.Tasks.UpdateMyWorkTask;

namespace Work.Api.Endpoints.Tasks;

/// <summary>
/// Work tasks over HTTP (spec work/001): the signed-in staff member's own board «Мої задачі» (R-1…R-3) and a task
/// the administrator sets for anybody (R-4).
/// </summary>
public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var own = endpoints.MapGroup("api/work")
            .WithTags("WorkTasks")
            .RequireAuthorization(PolicyNames.RequireManagerRole);
        GetMyWorkTasksEndpoint.Map(own);
        CreateMyWorkTaskEndpoint.Map(own);
        UpdateMyWorkTaskEndpoint.Map(own);
        ChangeMyWorkTaskStatusEndpoint.Map(own);
        SetMyWorkTaskStepEndpoint.Map(own);
        DeleteMyWorkTaskEndpoint.Map(own);

        var admin = endpoints.MapGroup("api/admin/work")
            .WithTags("AdminWork")
            .RequireAuthorization(PolicyNames.RequireAdminRole);
        CreateWorkTaskForUserEndpoint.Map(admin);

        return endpoints;
    }
}
