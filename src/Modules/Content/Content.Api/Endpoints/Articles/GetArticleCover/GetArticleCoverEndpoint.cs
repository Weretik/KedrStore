using Content.Application.Features.Articles.Queries.GetArticleCover;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
namespace Content.Api.Endpoints.Articles.GetArticleCover;

internal static class GetArticleCoverEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("articles/{id:long}/cover", Handle)
            .WithName("getArticleCover")
            // The document keeps the response type the controller declared, so the contract stays the same.
            .Produces<FileContentResult>(StatusCodes.Status200OK, "image/webp")
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Cover bytes. The URL carries the cover version, so a matching request is cached for a year.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromQuery] int? v, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetArticleCoverQuery(id), cancellationToken);
        if (!result.IsSuccess)
            return result.ToProblem(http);

        http.Response.Headers[HeaderNames.CacheControl] = v == result.Value.Version
            ? "public, max-age=31536000, immutable"
            : "public, max-age=300";
        return TypedResults.File(result.Value.Data, result.Value.ContentType);
    }
}
