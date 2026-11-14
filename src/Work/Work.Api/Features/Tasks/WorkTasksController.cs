using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Work.Application.Features.Report.Queries.GetWorkReport;
using Work.Application.Features.Tasks.Commands.ChangeMyWorkTaskStatus;
using Work.Application.Features.Tasks.Shared;
using Work.Application.Features.Tasks.Commands.CreateMyWorkTask;
using Work.Application.Features.Tasks.Commands.DeleteMyWorkTask;
using Work.Application.Features.Tasks.Queries.GetMyWorkTasks;
using Work.Application.Features.Tasks.Commands.SetMyWorkTaskStep;
using Work.Application.Features.Tasks.Commands.UpdateMyWorkTask;

namespace Work.Api.Features.Tasks;

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
        => this.ToActionResult(await sender.Send(new CreateMyWorkTaskCommand(
            request.Title, request.Details, request.DueDate, request.Steps), cancellationToken));

    [HttpPut("tasks/{id:long}", Name = "updateMyWorkTask")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTaskDto>> Update(
        [FromRoute] long id, [FromBody] SaveWorkTaskRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateMyWorkTaskCommand(
            id, request.Title, request.Details, request.DueDate, request.Steps), cancellationToken));

    [HttpPut("tasks/{id:long}/status", Name = "changeMyWorkTaskStatus")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTaskDto>> ChangeStatus(
        [FromRoute] long id, [FromBody] ChangeWorkTaskStatusRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new ChangeMyWorkTaskStatusCommand(id, request.Status, request.Result), cancellationToken));

    [HttpPut("tasks/{id:long}/steps/{index:int}", Name = "setMyWorkTaskStep")]
    [ProducesResponseType(typeof(WorkTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkTaskDto>> SetStep(
        [FromRoute] long id, [FromRoute] int index, [FromBody] SetWorkTaskStepRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new SetMyWorkTaskStepCommand(id, index, request.IsDone), cancellationToken));

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
