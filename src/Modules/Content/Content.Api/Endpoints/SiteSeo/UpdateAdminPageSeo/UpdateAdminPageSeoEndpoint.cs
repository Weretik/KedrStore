using Content.Application.Features.SiteSeo.Commands.UpdatePageSeo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.SiteSeo.Shared;

namespace Content.Api.Endpoints.SiteSeo.UpdateAdminPageSeo;

internal static class UpdateAdminPageSeoEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{page}", Handle)
            .WithName("updateAdminPageSeo")
            .WithSummary("Replace the texts of one page")
            .Produces<AdminPageSeoResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string page, [FromBody] SavePageSeoRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePageSeoCommand(page, request.TitleUk, request.TitleRu,
            request.DescriptionUk, request.DescriptionRu), cancellationToken);
        return result.Map(PageSeoResponseMapper.ToResponse).ToHttpResult(http);
    }
}
