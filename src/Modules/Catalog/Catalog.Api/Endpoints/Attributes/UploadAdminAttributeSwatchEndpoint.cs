using Catalog.Application.Features.Attributes.Commands.UploadAttributeSwatch;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Attributes;

internal static class UploadAdminAttributeSwatchEndpoint
{
    private const long SwatchRequestLimit = UploadAttributeSwatchValidator.MaxUploadBytes + 64 * 1024;

    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("swatches", Handle)
            .WithName("uploadAdminAttributeSwatch")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .WithMetadata(
                new RequestSizeLimitAttribute(SwatchRequestLimit),
                new RequestFormLimitsAttribute { MultipartBodyLengthLimit = SwatchRequestLimit })
            // The host's cookie CSRF check covers form posts, so the Minimal API antiforgery check stays off.
            .DisableAntiforgery()
            .Produces<AdminAttributeSwatchUploadDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status503ServiceUnavailable);

    /// <summary>
    /// Puts a color or decor picture into the media library (CDN folder <c>swatches</c>, library folder «Кольори»)
    /// before the value is saved; send the answer's <c>mediaFileId</c> as the value's <c>swatchMediaFileId</c>.
    /// </summary>
    private static async Task<IResult> Handle(
        IFormFile? file, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        // A missing file becomes null, which the validator answers with 400.
        var upload = file is null ? null! : new SwatchUpload(file.FileName, file.Length, file.OpenReadStream);
        return (await sender.Send(new UploadAttributeSwatchCommand(upload), cancellationToken)).ToHttpResult(http);
    }
}
