using Identity.Api.Contracts.Users;
using Identity.Application.Features.Users.ChangeStaffRole;
using Identity.Application.Features.Users.CreateStaff;
using Identity.Application.Features.Users.GetStaffList;
using Identity.Application.Features.Users.SetStaffBlocked;
using Identity.Application.Features.Users.SetStaffPassword;
using Identity.Application.Security.Policies;

namespace Identity.Api.Controllers;

/// <summary>
/// Staff accounts (administrators and managers), managed by an administrator.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = PolicyNames.CanManageUsers)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AdminUsersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getStaffAccounts")]
    [ProducesResponseType(typeof(IReadOnlyList<StaffAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StaffAccountResponse>>> GetList(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffListQuery(), cancellationToken);
        return Ok(result.Value.Select(StaffAccountResponse.From).ToList());
    }

    [HttpPost(Name = "createStaffAccount")]
    [ProducesResponseType(typeof(StaffAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffAccountResponse>> Create(
        [FromBody] CreateStaffAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateStaffCommand(request.Email, request.FullName, request.Role, request.Password),
            cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? StatusCode(StatusCodes.Status201Created, StaffAccountResponse.From(result.Value))
            : this.ToActionResult(result);
    }

    [HttpPut("{userId:guid}/role", Name = "changeStaffRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(Guid userId, [FromBody] ChangeStaffRoleRequest request, CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new ChangeStaffRoleCommand(userId, request.Role), cancellationToken));

    [HttpPut("{userId:guid}/blocked", Name = "setStaffBlocked")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetBlocked(Guid userId, [FromBody] SetStaffBlockedRequest request, CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new SetStaffBlockedCommand(userId, request.Blocked), cancellationToken));

    [HttpPut("{userId:guid}/password", Name = "setStaffPassword")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPassword(Guid userId, [FromBody] SetStaffPasswordRequest request, CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new SetStaffPasswordCommand(userId, request.Password), cancellationToken));

    private IActionResult NoContentOr(Ardalis.Result.Result result)
        => result.Status == Ardalis.Result.ResultStatus.Ok ? NoContent() : this.ToActionResult(result);
}
