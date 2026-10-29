using Identity.Api.Avatars;
using Identity.Api.Contracts.Profile;
using Identity.Application.Features.Profile.ChangeOwnEmail;
using Identity.Application.Features.Profile.ChangeOwnName;
using Identity.Application.Features.Profile.ChangeOwnPassword;
using Identity.Application.Features.Profile.GetProfile;
using Identity.Application.Features.Profile.RemoveOwnAvatar;
using Identity.Application.Features.Profile.UploadOwnAvatar;
using Identity.Application.Security.Policies;

namespace Identity.Api.Controllers;

/// <summary>
/// The signed-in staff account's own profile. Managers change name and avatar;
/// administrators also change email and password.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize(Policy = PolicyNames.RequireManagerRole)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class ProfileController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getMyProfile")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProfileQuery(), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Ok(ProfileResponse.From(result.Value))
            : this.ToActionResult(result);
    }

    [HttpPut("name", Name = "changeMyName")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeName([FromBody] ChangeNameRequest request, CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new ChangeOwnNameCommand(request.FullName), cancellationToken));

    [HttpPut("avatar", Name = "uploadMyAvatar")]
    [RequestSizeLimit(AvatarUploadResults.RequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = AvatarUploadResults.RequestLimit)]
    [ProducesResponseType(typeof(AvatarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
        => AvatarUploadResults.ToActionResult(
            this,
            await sender.Send(new UploadOwnAvatarCommand(AvatarUploadResults.ToUpload(file)), cancellationToken));

    [HttpDelete("avatar", Name = "removeMyAvatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new RemoveOwnAvatarCommand(), cancellationToken));

    [HttpPut("email", Name = "changeMyEmail")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeOwnEmailRequest request, CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new ChangeOwnEmailCommand(request.Email, request.CurrentPassword), cancellationToken));

    [HttpPut("password", Name = "changeMyPassword")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeOwnPasswordRequest request, CancellationToken cancellationToken)
        => NoContentOr(await sender.Send(new ChangeOwnPasswordCommand(request.CurrentPassword, request.NewPassword), cancellationToken));

    private IActionResult NoContentOr(Ardalis.Result.Result result)
        => result.Status == Ardalis.Result.ResultStatus.Ok ? NoContent() : this.ToActionResult(result);
}
