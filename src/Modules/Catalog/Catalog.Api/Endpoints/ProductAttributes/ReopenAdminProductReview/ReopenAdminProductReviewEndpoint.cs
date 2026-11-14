using Catalog.Application.Features.ProductAttributes.Commands.ReopenProductReview;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.ProductAttributes.ReopenAdminProductReview;

internal static class ReopenAdminProductReviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("review/reopen", Handle)
            .WithName("reopenAdminProductReview")
            .Produces<AdminProductAttributesDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>«Повернути на перевірку».</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int productId, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new ReopenProductReviewCommand(productId), cancellationToken)).ToHttpResult(http);
}
