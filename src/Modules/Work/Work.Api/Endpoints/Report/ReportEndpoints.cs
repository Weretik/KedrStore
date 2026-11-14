using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Work.Api.Endpoints.Report.GetMyWorkReport;

using Work.Api.Endpoints.Report.GetWorkReport;

namespace Work.Api.Endpoints.Report;

/// <summary>«Звіт роботи» (spec work/001, R-7): the staff member's own report and, for the administrator, anybody's.</summary>
public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        GetMyWorkReportEndpoint.Map(endpoints.MapGroup("api/work")
            .WithTags("WorkTasks")
            .RequireAuthorization(PolicyNames.RequireManagerRole));
        GetWorkReportEndpoint.Map(endpoints.MapGroup("api/admin/work")
            .WithTags("AdminWork")
            .RequireAuthorization(PolicyNames.RequireAdminRole));

        return endpoints;
    }
}
