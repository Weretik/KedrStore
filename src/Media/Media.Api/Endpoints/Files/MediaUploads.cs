using Microsoft.AspNetCore.Builder;
using Media.Application.Abstractions.Images;
using Media.Application.Features.Files.Shared;

namespace Media.Api.Endpoints.Files;

/// <summary>The multipart file of an upload or a replacement.</summary>
internal static class MediaUploads
{
    /// <summary>The largest picture plus room for multipart headers; larger bodies are refused before they are read.</summary>
    public const long RequestLimit = MediaUploadRules.MaxUploadBytes + 64 * 1024;

    /// <summary>The body limit of an upload endpoint, as <c>[RequestSizeLimit]</c> and <c>[RequestFormLimits]</c> gave it.</summary>
    public static RouteHandlerBuilder WithUploadLimit(this RouteHandlerBuilder endpoint) =>
        endpoint.WithMetadata(
            new RequestSizeLimitAttribute(RequestLimit),
            new RequestFormLimitsAttribute { MultipartBodyLengthLimit = RequestLimit });

    /// <summary>A missing file becomes <c>null</c>, which the validator answers with 400.</summary>
    public static MediaUpload ToUpload(IFormFile? file)
        => file is null ? null! : new(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
}
