using Content.Application.Features.SeoSettings.Queries.GetSiteSeoSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.SeoSettings;

internal static class GetAdminSiteSeoSettingsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminSiteSeoSettings")
            .Produces<AdminSiteSeoSettingsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken);
        return result.Map(SiteSeoSettingsResponseMapper.ToAdminResponse).ToHttpResult(http);
    }
}
