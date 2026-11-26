using Content.Application.Features.Overview.Queries.GetContentSeoReview;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Overview.GetAdminContentSeoReview;

internal static class GetAdminContentSeoReviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminContentSeoReview")
            .WithSummary("Article pages whose search texts need work")
            .WithDescription("Requires `CanManageContent` (Manager, Seo, or Admin). Spec platform/010: published articles per language; `kind` is `article`; `issues` holds `noDescription`, `longTitle`, `duplicateTitle`. Only articles with an issue are listed. Read-only.")
            .Produces<IReadOnlyList<ContentSeoReviewItem>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetContentSeoReviewQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}
