using Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;
using Catalog.Contracts.RetailPrices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.RetailPrices.ImportAdminRetailPrices;

internal static class ImportAdminRetailPricesEndpoint
{
    private const long RequestLimit = ImportRetailPricesValidator.MaxUploadBytes + 64 * 1024;

    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("import", Handle)
            .WithName("importAdminRetailPrices").RequireRateLimiting("HeavyOperation")
            .WithSummary("Set the site's retail prices from the 1C price list in Excel")
            .WithDescription("Policy CanManageProducts (Manager, Seo, and Admin). Multipart `file` (.xlsx up to 10 MB) with a header row holding «Код» and the retail price type's name; only that price type changes, by product code. `protectFromOneC` (default true) keeps the prices from the file from the 1C sync; false gives them back to it. The product list is rebuilt before the answer. 409 when a 1C sync changed the same prices meanwhile.")
            .WithMetadata(
                new RequestSizeLimitAttribute(RequestLimit),
                new RequestFormLimitsAttribute { MultipartBodyLengthLimit = RequestLimit })
            // The host's cookie CSRF check covers form posts, so the Minimal API antiforgery check stays off.
            .DisableAntiforgery()
            .Produces<RetailPriceImportResultDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

    /// <summary>Reads the uploaded price file; a missing file becomes null, which the validator answers with 400.</summary>
    private static async Task<IResult> Handle(IFormFile? file, [FromForm] bool? protectFromOneC, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
    {
        var upload = file is null ? null! : new RetailPriceUpload(file.FileName, file.Length, file.OpenReadStream);
        var command = new ImportRetailPricesCommand(upload, protectFromOneC ?? true);
        return (await sender.Send(command, cancellationToken)).ToHttpResult(http);
    }
}
