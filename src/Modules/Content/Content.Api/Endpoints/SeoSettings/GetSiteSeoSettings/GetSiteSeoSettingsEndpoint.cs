using Content.Application.Features.SeoSettings.Queries.GetSiteSeoSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.SeoSettings.Shared;

namespace Content.Api.Endpoints.SeoSettings.GetSiteSeoSettings;

internal static class GetSiteSeoSettingsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getSiteSeoSettings")
            .Produces<SiteSeoSettingsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken);
        return result.Map(SiteSeoSettingsResponseMapper.ToResponse).ToHttpResult(http);
    }
}
