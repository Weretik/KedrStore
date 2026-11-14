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
