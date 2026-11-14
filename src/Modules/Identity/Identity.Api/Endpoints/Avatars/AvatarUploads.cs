using Identity.Api.Endpoints.Profile;
using Identity.Application.Features.Avatars;
using Microsoft.AspNetCore.Builder;

using Identity.Api.Endpoints.Profile.Shared;
namespace Identity.Api.Endpoints.Avatars;

/// <summary>HTTP shape shared by the own-profile and Users-page avatar uploads.</summary>
internal static class AvatarUploads
{
    // The 2 MB file plus room for multipart headers; larger bodies are refused before they are read.
    public const long RequestLimit = AvatarRules.MaxBytes + 64 * 1024;

    /// <summary>
    /// The body limit, as <c>[RequestSizeLimit]</c> and <c>[RequestFormLimits]</c> gave it. The host's cookie CSRF
    /// check covers form posts, so the Minimal API antiforgery check stays off.
    /// </summary>
    public static RouteHandlerBuilder AsAvatarUpload(this RouteHandlerBuilder endpoint) =>
        endpoint
            .WithMetadata(
                new RequestSizeLimitAttribute(RequestLimit),
                new RequestFormLimitsAttribute { MultipartBodyLengthLimit = RequestLimit })
            .DisableAntiforgery()
            .Produces<AvatarResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status503ServiceUnavailable);

    /// <summary>A missing file becomes <c>null</c>, which the validator answers with 400.</summary>
    public static AvatarUpload ToUpload(IFormFile? file) => file is null ? null! : new(file.OpenReadStream(), file.Length);

    public static IResult ToHttpResult(Ardalis.Result.Result<AvatarResult> result, HttpContext http) =>
        result.Status == Ardalis.Result.ResultStatus.Ok
            ? TypedResults.Ok(new AvatarResponse(result.Value.AvatarUrl))
            : result.ToProblem(http);
}
