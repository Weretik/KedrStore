using Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.SeoSettings.Shared;

namespace Content.Api.Endpoints.SeoSettings.UpdateAdminSiteSeoSettings;

internal static class UpdateAdminSiteSeoSettingsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("", Handle)
            .WithName("updateAdminSiteSeoSettings")
            .WithSummary("Replace all settings")
            .WithDescription("Requires `CanManageContent` (Manager, Seo, or Admin). Ids are trimmed and upper-cased; for a verification code the whole meta tag may be sent and its `content` is kept. A blank or missing value removes that code or picture. A new picture must be in the media library. Repeating it is safe.")
            .Produces<AdminSiteSeoSettingsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromBody] SaveSiteSeoSettingsRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSiteSeoSettingsCommand(SiteSeoSettingsResponseMapper.ToInput(request)), cancellationToken);
        return result.Map(SiteSeoSettingsResponseMapper.ToAdminResponse).ToHttpResult(http);
    }
}
