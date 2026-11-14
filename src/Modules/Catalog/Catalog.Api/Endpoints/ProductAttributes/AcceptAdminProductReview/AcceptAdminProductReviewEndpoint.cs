using Catalog.Application.Features.ProductAttributes.Commands.AcceptProductReview;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.ProductAttributes.AcceptAdminProductReview;

internal static class AcceptAdminProductReviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("review/accept", Handle)
            .WithName("acceptAdminProductReview")
            .Produces<AdminProductAttributesDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>«Прийняти як є»: the product stops counting as a problem until a person reopens it.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int productId, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new AcceptProductReviewCommand(productId), cancellationToken)).ToHttpResult(http);
}
