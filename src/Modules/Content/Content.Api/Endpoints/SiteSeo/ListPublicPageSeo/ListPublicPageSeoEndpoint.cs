using Content.Application.Features.SiteSeo.Queries.ListPublicPageSeo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.SiteSeo.Shared;

namespace Content.Api.Endpoints.SiteSeo.ListPublicPageSeo;

internal static class ListPublicPageSeoEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/page-seo", Handle)
            .WithName("listPublicPageSeo")
            .WithSummary("List the set search texts in one language")
            .WithDescription("Public, because the site builds page metadata for anonymous visitors. Only pages with a title or a description in this language are listed.")
            .Produces<IReadOnlyList<PublicPageSeoResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPublicPageSeoQuery(lang), cancellationToken);
        return result.Map(PageSeoResponseMapper.ToResponse).ToHttpResult(http);
    }
}
