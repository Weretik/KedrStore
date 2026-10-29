using Identity.Api.Contracts.Profile;
using Identity.Application.Features.Avatars;

namespace Identity.Api.Avatars;

/// <summary>HTTP shape shared by the own-profile and Users-page avatar uploads.</summary>
internal static class AvatarUploadResults
{
    // The 2 MB file plus room for multipart headers; larger bodies are refused before they are read.
    public const long RequestLimit = AvatarRules.MaxBytes + 64 * 1024;

    public static AvatarUpload ToUpload(IFormFile file) => new(file.OpenReadStream(), file.Length);

    public static IActionResult ToActionResult(ControllerBase controller, Ardalis.Result.Result<AvatarResult> result) =>
        result.Status switch
        {
            Ardalis.Result.ResultStatus.Ok => controller.Ok(new AvatarResponse(result.Value.AvatarUrl)),
            Ardalis.Result.ResultStatus.Unavailable => controller.StatusCode(StatusCodes.Status503ServiceUnavailable, result.Errors),
            _ => controller.ToActionResult(result)
        };
}
