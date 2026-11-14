using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Commands.UploadArticleCover;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Articles;

internal static class UploadAdminArticleCoverEndpoint
{
    private const long CoverRequestLimit = UploadArticleCoverValidator.MaxUploadBytes + 64 * 1024;

    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("covers", Handle)
            .WithName("uploadAdminArticleCover")
            .WithMetadata(
                new RequestSizeLimitAttribute(CoverRequestLimit),
                new RequestFormLimitsAttribute { MultipartBodyLengthLimit = CoverRequestLimit })
            // The host's cookie CSRF check covers form posts, so the Minimal API antiforgery check stays off.
            .DisableAntiforgery()
            .Produces<ArticleCoverUploadResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status503ServiceUnavailable);

    /// <summary>
    /// Puts a cover picture into the media library (CDN folder <c>articles</c>, library folder «Статті») before the
    /// article is saved; send the answer's <c>mediaFileId</c> with the create or update.
    /// </summary>
    private static async Task<IResult> Handle(
        IFormFile? file, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        // A missing file becomes null, which the validator answers with 400.
        var upload = file is null
            ? null!
            : new CoverUpload(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
        var result = await sender.Send(new UploadArticleCoverCommand(upload), cancellationToken);
        return result.Map(ArticleResponseMapper.ToResponse).ToHttpResult(http);
    }
}
