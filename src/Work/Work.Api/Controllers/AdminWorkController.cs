using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;
using Work.Api.Requests;
using Work.Application.Features.Report.GetReport;
using Work.Application.Features.Tasks.Common;
using Work.Application.Features.Tasks.CreateForUser;

namespace Work.Api.Controllers;

/// <summary>«Звіт роботи» for the administrator: anybody's report and a task for anybody (spec work/001, R-4, R-7).</summary>
[ApiController]
[Route("api/admin/work")]
[Authorize(Policy = PolicyNames.RequireAdminRole)]
public sealed class AdminWorkController(ISender sender) : ControllerBase
{
    [HttpPost("tasks", Name = "createAdminWorkTask")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkTaskDto>> CreateTask(
        [FromBody] CreateWorkTaskForUserRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new CreateWorkTaskForUserCommand(
                request.OwnerUserId, request.Title, request.Details, request.DueDate, request.Steps), cancellationToken));

    [HttpGet("report", Name = "getAdminWorkReport")]
    [ProducesResponseType(typeof(WorkReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkReportDto>> GetReport(
        [FromQuery] Guid userId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetWorkReportQuery(userId, from, to), cancellationToken));
}
