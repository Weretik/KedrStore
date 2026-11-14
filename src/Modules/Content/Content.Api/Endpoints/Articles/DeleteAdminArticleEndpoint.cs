using Content.Application.Features.Articles.Commands.DeleteArticle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Articles;

internal static class DeleteAdminArticleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", Handle)
            .WithName("deleteAdminArticle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteArticleCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToHttpResult(http);
    }
}
