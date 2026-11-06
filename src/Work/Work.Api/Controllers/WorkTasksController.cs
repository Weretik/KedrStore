using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;
using Work.Api.Requests;
using Work.Application.Features.Report.GetReport;
using Work.Application.Features.Tasks.ChangeStatus;
using Work.Application.Features.Tasks.Common;
using Work.Application.Features.Tasks.Create;
using Work.Application.Features.Tasks.Delete;
using Work.Application.Features.Tasks.GetMine;
using Work.Application.Features.Tasks.Update;

namespace Work.Api.Controllers;

/// <summary>The signed-in staff member's own board «Мої задачі» and own report (spec work/001, R-1…R-3, R-7).</summary>
[ApiController]
[Route("api/work")]
[Authorize(Policy = PolicyNames.RequireManagerRole)]
public sealed class WorkTasksController(ISender sender) : ControllerBase
{
    [HttpGet("tasks", Name = "getMyWorkTasks")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkTaskDto>>> GetTasks(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetMyWorkTasksQuery(), cancellationToken));

    [HttpPost("tasks", Name = "createMyWorkTask")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkTaskDto>> Create(
        [FromBody] SaveWorkTaskRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateMyWorkTaskCommand(request.Title, request.Details), cancellationToken));

    [HttpPut("tasks/{id:long}", Name = "updateMyWorkTask")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTaskDto>> Update(
        [FromRoute] long id, [FromBody] SaveWorkTaskRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateMyWorkTaskCommand(id, request.Title, request.Details), cancellationToken));

    [HttpPut("tasks/{id:long}/status", Name = "changeMyWorkTaskStatus")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTaskDto>> ChangeStatus(
        [FromRoute] long id, [FromBody] ChangeWorkTaskStatusRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new ChangeMyWorkTaskStatusCommand(id, request.Status, request.Result), cancellationToken));

    [HttpDelete("tasks/{id:long}", Name = "deleteMyWorkTask")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMyWorkTaskCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    [HttpGet("report", Name = "getMyWorkReport")]
    [ProducesResponseType(typeof(WorkReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkReportDto>> GetReport(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetWorkReportQuery(null, from, to), cancellationToken));
}
